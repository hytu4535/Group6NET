"""Datasets.

`make_synthetic` produces separable-but-noisy identities so the whole pipeline can be
proven end-to-end *without* the gated PetFace download. Each identity has a distinct
low-frequency template; samples add noise/brightness/shift (intra-class variation).
Real datasets (PetFace, nose-print) plug in behind the same (images, labels) contract.

Splits are BY INDIVIDUAL (never by image) per docs/DATA.md, and we hold out a set of
*unknown* identities that appear only at query time to test open-set rejection.
"""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np


@dataclass
class SplitData:
    # all arrays: images (N,C,H,W) float32 in [0,1]; labels (N,) int
    train_x: np.ndarray
    train_y: np.ndarray
    gallery_x: np.ndarray
    gallery_y: np.ndarray
    query_x: np.ndarray
    query_y: np.ndarray
    known_ids: set[int]
    unknown_ids: set[int]

    def is_known(self, y: np.ndarray) -> np.ndarray:
        known = np.array(sorted(self.known_ids))
        return np.isin(y, known)


def _identity_template(rng: np.random.Generator, size: int, channels: int) -> np.ndarray:
    """A distinctive low-frequency pattern, upsampled to (channels,size,size)."""
    base = rng.random((channels, 8, 8)).astype(np.float32)
    # nearest-neighbour upsample 8 -> size
    reps = int(np.ceil(size / 8))
    up = np.repeat(np.repeat(base, reps, axis=1), reps, axis=2)[:, :size, :size]
    return up


def _sample_from_template(rng: np.random.Generator, tmpl: np.ndarray,
                          noise: float = 0.15) -> np.ndarray:
    img = tmpl.copy()
    img += rng.normal(0, noise, img.shape).astype(np.float32)        # capture noise
    img *= rng.uniform(0.8, 1.2)                                     # brightness
    shift = rng.integers(-3, 4, size=2)                             # small translation
    img = np.roll(img, shift=(shift[0], shift[1]), axis=(1, 2))
    return np.clip(img, 0.0, 1.0)


def make_synthetic(
    num_known: int = 40,
    num_unknown: int = 10,
    per_id_train: int = 12,
    per_id_gallery: int = 3,
    per_id_query: int = 3,
    img_size: int = 64,
    channels: int = 3,
    seed: int = 1337,
) -> SplitData:
    rng = np.random.default_rng(seed)
    total_ids = num_known + num_unknown
    templates = {i: _identity_template(rng, img_size, channels) for i in range(total_ids)}
    known_ids = set(range(num_known))
    unknown_ids = set(range(num_known, total_ids))

    def stack(ids, n):
        xs, ys = [], []
        for i in ids:
            for _ in range(n):
                xs.append(_sample_from_template(rng, templates[i]))
                ys.append(i)
        if not xs:                                  # empty split (e.g. num_unknown=0)
            return (np.empty((0, channels, img_size, img_size), np.float32),
                    np.empty((0,), np.int64))
        return np.asarray(xs, dtype=np.float32), np.asarray(ys, dtype=np.int64)

    train_x, train_y = stack(known_ids, per_id_train)        # known only
    gallery_x, gallery_y = stack(known_ids, per_id_gallery)  # enrolled = known only
    # query = fresh known samples (should match) + unknown samples (should be rejected)
    qk_x, qk_y = stack(known_ids, per_id_query)
    qu_x, qu_y = stack(unknown_ids, per_id_query)
    query_x = np.concatenate([qk_x, qu_x], axis=0)
    query_y = np.concatenate([qk_y, qu_y], axis=0)

    return SplitData(train_x, train_y, gallery_x, gallery_y, query_x, query_y,
                     known_ids, unknown_ids)


class ArrayDataset:
    """Minimal torch Dataset over numpy arrays (lazy torch import).

    With augment=True, applies on-the-fly train-time augmentation (flip, random-resized-crop,
    colour jitter, small rotation) — essential for real, few-images-per-individual data to avoid
    memorising the handful of training photos. Gracefully no-ops if torchvision is unavailable.
    """

    def __init__(self, x: np.ndarray, y: np.ndarray, augment: bool = False):
        import torch
        self.x = torch.from_numpy(x)
        self.y = torch.from_numpy(y)
        self.tf = None
        if augment:
            try:
                from torchvision.transforms import v2
                h, w = x.shape[2], x.shape[3]
                self.tf = v2.Compose([
                    v2.RandomHorizontalFlip(),
                    v2.RandomResizedCrop((h, w), scale=(0.7, 1.0), antialias=True),
                    v2.ColorJitter(0.2, 0.2, 0.2, 0.03),
                    v2.RandomRotation(12),
                ])
            except Exception:
                self.tf = None          # torchvision missing -> train without augmentation

    def __len__(self):
        return len(self.y)

    def __getitem__(self, idx):
        img = self.x[idx]
        if self.tf is not None:
            img = self.tf(img)
        return img, self.y[idx]
