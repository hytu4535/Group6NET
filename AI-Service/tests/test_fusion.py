import numpy as np
import pytest

pytest.importorskip("torch")

from petsy.config import Config                         # noqa: E402
from petsy.data import make_synthetic                   # noqa: E402
from petsy.fusion import extract_fused, train_fusion    # noqa: E402
from petsy.metrics import rank_k_and_map                # noqa: E402


def _second_view(x, seed=7):
    """Synthetic 'face' view = nose view + extra noise (correlated, not identical)."""
    rng = np.random.default_rng(seed)
    return np.clip(x + rng.normal(0, 0.1, x.shape).astype(np.float32), 0, 1)


def test_fusion_trains_and_matches():
    cfg = Config(head="arcface", epochs=8, embed_dim=32)
    s = make_synthetic(num_known=20, num_unknown=0, seed=cfg.seed)

    model, hist = train_fusion(cfg, s.train_x, _second_view(s.train_x), s.train_y, verbose=False)
    assert hist[-1] < hist[0]                              # loss decreased

    g = extract_fused(model, s.gallery_x, _second_view(s.gallery_x), cfg.resolved_device())
    q = extract_fused(model, s.query_x, _second_view(s.query_x), cfg.resolved_device())
    assert g.shape[1] == cfg.embed_dim * 2                 # fused dim = 2x

    out = rank_k_and_map(q, s.query_y, g, s.gallery_y, ks=(1,))
    assert out["rank_1"] > 0.5                              # well above chance
