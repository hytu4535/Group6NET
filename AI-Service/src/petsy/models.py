"""Embedding backbone + metric-learning heads (ArcFace, MagFace).

This is the heart of Biometric-Pet-Identification's selected method (docs/research/METHOD_BENCHMARK.md):
- ArcFace = the stable baseline (start here).
- MagFace = the upgrade that encodes capture *quality* into feature magnitude — the
  recipe behind the ~99.8% dog nose-print result, and a free quality signal for stage [0].

Heads return (logits, reg_loss). reg_loss is 0 for ArcFace/softmax; MagFace adds its
magnitude regulariser. The backbone outputs a RAW embedding (un-normalised) so MagFace
can read its magnitude; use `l2_normalize` for inference/matching.
"""
from __future__ import annotations

import math

import torch
import torch.nn as nn
import torch.nn.functional as F


# ---------------------------------------------------------------- backbones
class SmallCNN(nn.Module):
    """Tiny 6 GB-friendly CNN for synthetic sanity + CPU runs."""

    def __init__(self, channels: int = 3, embed_dim: int = 128):
        super().__init__()
        self.features = nn.Sequential(
            nn.Conv2d(channels, 32, 3, padding=1), nn.BatchNorm2d(32), nn.ReLU(),
            nn.MaxPool2d(2),
            nn.Conv2d(32, 64, 3, padding=1), nn.BatchNorm2d(64), nn.ReLU(),
            nn.MaxPool2d(2),
            nn.Conv2d(64, 128, 3, padding=1), nn.BatchNorm2d(128), nn.ReLU(),
            nn.AdaptiveAvgPool2d(1),
        )
        self.proj = nn.Linear(128, embed_dim)

    def forward(self, x):
        h = self.features(x).flatten(1)
        return self.proj(h)


def get_backbone(name: str, channels: int, embed_dim: int) -> nn.Module:
    """Pluggable backbone. Real training swaps in timm EfficientNet/Tiny-ViT/ResNeSt."""
    if name == "small_cnn":
        return SmallCNN(channels, embed_dim)
    if name.startswith("timm:"):
        import timm
        return timm.create_model(name[5:], pretrained=True, num_classes=embed_dim,
                                 in_chans=channels)
    if name.startswith("torchvision:"):
        import torchvision
        ctor = getattr(torchvision.models, name[len("torchvision:"):])
        net = ctor(weights=None)
        if hasattr(net, "fc"):
            net.fc = nn.Linear(net.fc.in_features, embed_dim)
        return net
    raise ValueError(f"unknown backbone: {name}")


# ---------------------------------------------------------------- heads
class ArcFaceHead(nn.Module):
    """Additive Angular Margin (Deng et al., CVPR 2019)."""

    def __init__(self, embed_dim: int, num_classes: int, margin: float = 0.5,
                 scale: float = 30.0):
        super().__init__()
        self.W = nn.Parameter(torch.empty(num_classes, embed_dim))
        nn.init.xavier_uniform_(self.W)
        self.margin = margin
        self.scale = scale

    def forward(self, emb: torch.Tensor, labels: torch.Tensor):
        x = F.normalize(emb, dim=1)
        w = F.normalize(self.W, dim=1)
        cos = torch.clamp(x @ w.t(), -1 + 1e-7, 1 - 1e-7)
        theta = torch.acos(cos)
        target = F.one_hot(labels, num_classes=w.size(0)).float()
        marged = torch.cos(theta + self.margin * target)
        logits = self.scale * marged
        return logits, emb.new_zeros(())


class MagFaceHead(nn.Module):
    """MagFace (Meng et al., CVPR 2021): magnitude-aware margin + regulariser."""

    def __init__(self, embed_dim: int, num_classes: int, l_margin: float = 0.45,
                 u_margin: float = 0.8, l_a: float = 10.0, u_a: float = 110.0,
                 scale: float = 30.0, lambda_g: float = 20.0):
        super().__init__()
        self.W = nn.Parameter(torch.empty(num_classes, embed_dim))
        nn.init.xavier_uniform_(self.W)
        self.l_margin, self.u_margin = l_margin, u_margin
        self.l_a, self.u_a = l_a, u_a
        self.scale, self.lambda_g = scale, lambda_g

    def _m(self, a):  # linear margin schedule in feature magnitude
        return (self.u_margin - self.l_margin) / (self.u_a - self.l_a) * (a - self.l_a) + self.l_margin

    def _g(self, a):  # regulariser: reward larger magnitude (high quality)
        return 1.0 / a + a / (self.u_a ** 2)

    def forward(self, emb: torch.Tensor, labels: torch.Tensor):
        a = torch.clamp(torch.norm(emb, dim=1, keepdim=True), self.l_a, self.u_a)
        m = self._m(a).squeeze(1)
        reg = self.lambda_g * self._g(a).mean()
        x = F.normalize(emb, dim=1)
        w = F.normalize(self.W, dim=1)
        cos = torch.clamp(x @ w.t(), -1 + 1e-7, 1 - 1e-7)
        theta = torch.acos(cos)
        target = F.one_hot(labels, num_classes=w.size(0)).float()
        marged = torch.cos(theta + m.unsqueeze(1) * target)
        logits = self.scale * marged
        return logits, reg


class SoftmaxHead(nn.Module):
    def __init__(self, embed_dim: int, num_classes: int):
        super().__init__()
        self.fc = nn.Linear(embed_dim, num_classes)

    def forward(self, emb, labels):
        return self.fc(emb), emb.new_zeros(())


def build_head(name: str, embed_dim: int, num_classes: int, cfg) -> nn.Module:
    if name == "arcface":
        return ArcFaceHead(embed_dim, num_classes, cfg.arc_margin, cfg.arc_scale)
    if name == "magface":
        return MagFaceHead(embed_dim, num_classes, cfg.mag_l_margin, cfg.mag_u_margin,
                           cfg.mag_l_a, cfg.mag_u_a, cfg.arc_scale, cfg.mag_lambda_g)
    if name == "softmax":
        return SoftmaxHead(embed_dim, num_classes)
    raise ValueError(f"unknown head: {name}")


def l2_normalize(x: torch.Tensor) -> torch.Tensor:
    return F.normalize(x, dim=1)


def fuse(*embeddings: torch.Tensor) -> torch.Tensor:
    """Fusion of nose ⊕ face branches (MDPI 2023): concat then re-normalise."""
    return l2_normalize(torch.cat([l2_normalize(e) for e in embeddings], dim=1))
