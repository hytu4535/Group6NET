"""Open-set decision layer (ADR 0003).

Turns a raw similarity into a calibrated probability + a 3-way verdict:
  match | possible | no confident match
The threshold is chosen to satisfy a false-match-rate ceiling FIRST (never expose the
wrong owner), then an abstain band makes borderline cases "possible" rather than asserted.
"""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np

from .metrics import threshold_for_fmr


@dataclass
class OpenSetDecider:
    threshold: float
    abstain_band: float = 0.05

    @classmethod
    def fit(cls, val_scores, val_labels, target_fmr=0.01, abstain_band=0.05):
        thr = threshold_for_fmr(val_scores, val_labels, target_fmr)
        return cls(threshold=thr, abstain_band=abstain_band)

    def verdict(self, sim: float) -> str:
        if sim >= self.threshold + self.abstain_band:
            return "match"
        if sim >= self.threshold - self.abstain_band:
            return "possible"
        return "no confident match"

    def verdicts(self, sims) -> list[str]:
        return [self.verdict(float(s)) for s in np.asarray(sims)]


def platt_fit(scores, labels):
    """Logistic (Platt) calibration: similarity -> P(same identity). Pure-numpy GD."""
    s = np.asarray(scores, dtype=np.float64)
    y = np.asarray(labels, dtype=np.float64)
    a, b = 1.0, 0.0
    for _ in range(2000):
        z = a * s + b
        p = 1.0 / (1.0 + np.exp(-z))
        ga = np.mean((p - y) * s)
        gb = np.mean(p - y)
        a -= 0.1 * ga
        b -= 0.1 * gb
    return a, b


def platt_proba(scores, ab):
    a, b = ab
    return 1.0 / (1.0 + np.exp(-(a * np.asarray(scores) + b)))
