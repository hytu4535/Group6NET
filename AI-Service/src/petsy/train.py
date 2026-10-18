"""Training loop + embedding extraction.

Metric-learning training: backbone -> raw embedding -> head(logits) -> cross-entropy
(+ MagFace regulariser). Inference uses L2-normalised embeddings for cosine matching.
AMP is enabled on CUDA (the 6 GB RTX 3060 path), auto-disabled on CPU.
"""
from __future__ import annotations

import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import DataLoader

from .config import Config, set_seed
from .data import ArrayDataset
from .models import build_head, get_backbone, l2_normalize


def build(cfg: Config, num_classes: int):
    backbone = get_backbone(cfg.backbone, cfg.channels, cfg.embed_dim)
    head = build_head(cfg.head, cfg.embed_dim, num_classes, cfg)
    return backbone, head


def train(cfg: Config, train_x: np.ndarray, train_y: np.ndarray, *, verbose=True):
    set_seed(cfg.seed)                       # reproducible regardless of caller (scrutinize fix)
    device = cfg.resolved_device()
    use_amp = cfg.amp and device == "cuda"
    num_classes = int(train_y.max()) + 1

    backbone, head = build(cfg, num_classes)
    backbone.to(device); head.to(device)
    params = list(backbone.parameters()) + list(head.parameters())
    opt = torch.optim.AdamW(params, lr=cfg.lr, weight_decay=cfg.weight_decay)
    scaler = torch.amp.GradScaler("cuda", enabled=use_amp)
    ce = nn.CrossEntropyLoss()

    loader = DataLoader(ArrayDataset(train_x, train_y, augment=cfg.augment),
                        batch_size=cfg.batch_size, shuffle=True, drop_last=False)

    backbone.train(); head.train()
    history = []
    for epoch in range(cfg.epochs):
        total, n = 0.0, 0
        for xb, yb in loader:
            xb, yb = xb.to(device), yb.to(device)
            opt.zero_grad(set_to_none=True)
            with torch.autocast(device_type="cuda", enabled=use_amp):
                emb = backbone(xb)
                logits, reg = head(emb, yb)
                loss = ce(logits, yb) + reg
            scaler.scale(loss).backward()
            scaler.step(opt)
            scaler.update()
            total += float(loss.detach()) * len(yb); n += len(yb)
        history.append(total / max(n, 1))
        if verbose:
            print(f"  epoch {epoch+1}/{cfg.epochs}  loss={history[-1]:.4f}")
    return backbone, history


@torch.no_grad()
def extract_embeddings(backbone, x: np.ndarray, device: str, batch=256) -> np.ndarray:
    backbone.eval()
    out = []
    xt = torch.from_numpy(x)
    for i in range(0, len(xt), batch):
        chunk = xt[i:i + batch].to(device)
        emb = l2_normalize(backbone(chunk))
        out.append(emb.cpu().numpy())
    return np.concatenate(out, axis=0).astype(np.float32)
