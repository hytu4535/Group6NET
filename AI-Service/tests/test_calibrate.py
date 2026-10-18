import numpy as np

from petsy.calibrate import OpenSetDecider, platt_fit, platt_proba


def test_verdict_bands():
    d = OpenSetDecider(threshold=0.5, abstain_band=0.05)
    assert d.verdict(0.7) == "match"
    assert d.verdict(0.5) == "possible"
    assert d.verdict(0.2) == "no confident match"


def test_fit_rejects_impostors():
    # genuine high, impostor low
    scores = np.array([0.8, 0.85, 0.9, 0.1, 0.15, 0.2])
    labels = np.array([1, 1, 1, 0, 0, 0])
    d = OpenSetDecider.fit(scores, labels, target_fmr=0.0, abstain_band=0.02)
    # an impostor-level score should not be "match"
    assert d.verdict(0.2) != "match"
    # a clear genuine score should be "match"
    assert d.verdict(0.9) == "match"


def test_platt_monotonic():
    scores = np.array([0.1, 0.2, 0.8, 0.9])
    labels = np.array([0, 0, 1, 1])
    ab = platt_fit(scores, labels)
    p = platt_proba(np.array([0.0, 1.0]), ab)
    assert p[1] > p[0]  # higher similarity -> higher P(same)
