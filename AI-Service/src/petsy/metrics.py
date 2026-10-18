"""Re-ID and open-set metrics (definitions in docs/EVALUATION.md).

- Rank-k / mAP: closed-set ranking quality.
- EER + FMR@recall: open-set verification (the safety-critical case — a found stray may
  be unenrolled and must be rejected, not matched to the wrong owner).
"""
from __future__ import annotations

import numpy as np


def rank_k_and_map(query_emb, query_y, gallery_emb, gallery_y, ks=(1, 5)):
    """Closed-set metrics over KNOWN queries only. Returns dict."""
    q = _norm(query_emb)
    g = _norm(gallery_emb)
    sims = q @ g.T                         # (Nq, Ng)
    order = np.argsort(-sims, axis=1)
    gy = np.asarray(gallery_y)
    matches = gy[order] == np.asarray(query_y)[:, None]   # (Nq, Ng) boolean

    out = {}
    for k in ks:
        out[f"rank_{k}"] = float(matches[:, :k].any(axis=1).mean())

    # mean average precision
    aps = []
    for row in matches:
        rel = np.flatnonzero(row)
        if rel.size == 0:
            aps.append(0.0)
            continue
        precisions = np.cumsum(row)[rel] / (rel + 1)
        aps.append(precisions.mean())
    out["mAP"] = float(np.mean(aps))
    return out


def verification_scores(query_emb, query_y, gallery_emb, gallery_y):
    """Top-1 gallery similarity per query + whether that top-1 is a true same-id match.
    Used for open-set EER / threshold selection."""
    q = _norm(query_emb)
    g = _norm(gallery_emb)
    sims = q @ g.T
    top1 = np.argmax(sims, axis=1)
    top1_sim = sims[np.arange(len(q)), top1]
    top1_same = (np.asarray(gallery_y)[top1] == np.asarray(query_y))
    return top1_sim, top1_same


def eer(scores, labels):
    """Equal Error Rate from similarity scores and binary same/diff labels."""
    scores = np.asarray(scores)
    labels = np.asarray(labels).astype(bool)
    thr = np.unique(scores)
    best = (1.0, 0.5)
    for t in thr:
        pred = scores >= t
        far = np.mean(pred[~labels]) if (~labels).any() else 0.0   # false accept
        frr = np.mean(~pred[labels]) if labels.any() else 0.0      # false reject
        if abs(far - frr) < best[0]:
            best = (abs(far - frr), (far + frr) / 2)
    return float(best[1])


def threshold_for_fmr(scores, labels, target_fmr=0.01):
    """Lowest threshold whose false-match-rate (on impostors) <= target. Safety-first."""
    scores = np.asarray(scores)
    labels = np.asarray(labels).astype(bool)
    imp = scores[~labels]
    if imp.size == 0:
        return float(np.min(scores))
    # threshold = quantile of impostor scores at (1 - target_fmr)
    return float(np.quantile(imp, 1.0 - target_fmr))


def _norm(x):
    x = np.asarray(x, dtype=np.float32)
    return x / np.clip(np.linalg.norm(x, axis=1, keepdims=True), 1e-12, None)
