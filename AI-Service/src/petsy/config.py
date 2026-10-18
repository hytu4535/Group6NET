"""Central configuration + reproducibility helpers."""
from __future__ import annotations

import os
import random
from dataclasses import dataclass, field

import numpy as np


@dataclass
class Config:
    # data
    img_size: int = 64
    channels: int = 3
    embed_dim: int = 128

    # model
    backbone: str = "small_cnn"          # small_cnn | timm:<name> | torchvision:<name>
    head: str = "arcface"                # arcface | magface | softmax
    arc_margin: float = 0.5
    arc_scale: float = 30.0
    # magface bounds (l_margin, u_margin, l_a, u_a) per Meng et al. 2021
    mag_l_margin: float = 0.45
    mag_u_margin: float = 0.8
    mag_l_a: float = 10.0
    mag_u_a: float = 110.0
    mag_lambda_g: float = 20.0

    # train
    epochs: int = 8
    batch_size: int = 64
    lr: float = 1e-3
    weight_decay: float = 1e-4
    amp: bool = True                     # auto-disabled on CPU
    augment: bool = True                 # train-time augmentation (critical for real, few-shot data)
    device: str = "auto"                 # auto | cuda | cpu

    # eval / matching
    rank_ks: tuple[int, ...] = (1, 5)
    open_set_threshold: float | None = None   # None => choose on val to hit FMR target
    target_fmr: float = 0.01                  # open-set false-match-rate ceiling (safety-first)
    abstain_band: float = 0.05                # +/- around threshold => "possible match"

    # repro
    seed: int = 1337

    def resolved_device(self) -> str:
        if self.device != "auto":
            return self.device
        try:
            import torch
            return "cuda" if torch.cuda.is_available() else "cpu"
        except Exception:
            return "cpu"


def set_seed(seed: int = 1337, *, deterministic: bool = True) -> None:
    """Make a run reproducible (EVALUATION.md requires seeded, deterministic results)."""
    os.environ["PYTHONHASHSEED"] = str(seed)
    random.seed(seed)
    np.random.seed(seed)
    try:
        import torch
        torch.manual_seed(seed)
        torch.cuda.manual_seed_all(seed)
        if deterministic:
            torch.backends.cudnn.deterministic = True
            torch.backends.cudnn.benchmark = False
    except Exception:
        pass
