"""Stage [0] — capture quality gate (the Google-DR Thailand lesson, productised).

Rejects ungradable inputs BEFORE matching. v1 implements a blur check (variance of a
Laplacian-like operator) + brightness check, all numpy. Real version adds occlusion,
pose, and species checks, and for video picks the most-frontal frame (Nose-to-ID).
"""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np


@dataclass
class QualityResult:
    ok: bool
    reason: str
    sharpness: float
    brightness: float


def _to_gray(img: np.ndarray) -> np.ndarray:
    # img: (C,H,W) or (H,W) float in [0,1]
    if img.ndim == 3:
        return img.mean(axis=0)
    return img


def _laplacian_var(gray: np.ndarray) -> float:
    k = np.array([[0, 1, 0], [1, -4, 1], [0, 1, 0]], dtype=np.float32)
    h, w = gray.shape
    out = np.zeros_like(gray)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            ky, kx = dy + 1, dx + 1
            if k[ky, kx] == 0:
                continue
            shifted = np.roll(np.roll(gray, dy, axis=0), dx, axis=1)
            out += k[ky, kx] * shifted
    return float(out[1:h - 1, 1:w - 1].var())


def quality_gate(img: np.ndarray, *, min_sharpness=1e-4, min_brightness=0.05,
                 max_brightness=0.97) -> QualityResult:
    gray = _to_gray(np.asarray(img, dtype=np.float32))
    sharp = _laplacian_var(gray)
    bright = float(gray.mean())
    if bright < min_brightness:
        return QualityResult(False, "too dark", sharp, bright)
    if bright > max_brightness:
        return QualityResult(False, "overexposed", sharp, bright)
    if sharp < min_sharpness:
        return QualityResult(False, "too blurry", sharp, bright)
    return QualityResult(True, "ok", sharp, bright)
