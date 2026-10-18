import numpy as np

from petsy.metrics import eer, rank_k_and_map, threshold_for_fmr, verification_scores


def test_rank1_perfect_on_separable():
    # gallery has one vector per id; queries identical to gallery -> rank-1 = 1.0
    g_emb = np.array([[1, 0], [0, 1]], dtype=np.float32)
    g_y = np.array([0, 1])
    q_emb = g_emb.copy()
    q_y = g_y.copy()
    out = rank_k_and_map(q_emb, q_y, g_emb, g_y, ks=(1,))
    assert out["rank_1"] == 1.0
    assert out["mAP"] == 1.0


def test_eer_zero_when_separable():
    # genuine sims high, impostor sims low -> EER ~ 0
    sims = np.array([0.9, 0.95, 0.1, 0.05])
    labels = np.array([1, 1, 0, 0])
    assert eer(sims, labels) < 0.01


def test_threshold_for_fmr_above_impostors():
    sims = np.array([0.9, 0.95, 0.1, 0.2])
    labels = np.array([1, 1, 0, 0])
    thr = threshold_for_fmr(sims, labels, target_fmr=0.0)
    assert thr >= 0.2  # must exclude all impostors at fmr=0


def test_verification_scores_shapes():
    g_emb = np.array([[1, 0], [0, 1]], dtype=np.float32)
    g_y = np.array([0, 1])
    q_emb = np.array([[1, 0]], dtype=np.float32)
    q_y = np.array([0])
    sim, same = verification_scores(q_emb, q_y, g_emb, g_y)
    assert sim.shape == (1,) and bool(same[0]) is True
