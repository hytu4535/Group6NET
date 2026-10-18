import numpy as np
import pytest

torch = pytest.importorskip("torch")

from petsy.config import Config              # noqa: E402
from petsy.models import build_head, fuse, get_backbone, l2_normalize  # noqa: E402


def test_backbone_output_shape():
    net = get_backbone("small_cnn", channels=3, embed_dim=32)
    x = torch.randn(4, 3, 64, 64)
    out = net(x)
    assert out.shape == (4, 32)


@pytest.mark.parametrize("head", ["arcface", "magface", "softmax"])
def test_head_forward(head):
    cfg = Config(embed_dim=16)
    h = build_head(head, embed_dim=16, num_classes=5, cfg=cfg)
    emb = torch.randn(8, 16)
    labels = torch.randint(0, 5, (8,))
    logits, reg = h(emb, labels)
    assert logits.shape == (8, 5)
    assert reg.ndim == 0  # scalar


def test_l2_normalize_unit_norm():
    x = torch.randn(10, 8)
    n = l2_normalize(x).norm(dim=1)
    assert torch.allclose(n, torch.ones(10), atol=1e-5)


def test_fuse_concats_and_normalizes():
    a = torch.randn(3, 4)
    b = torch.randn(3, 6)
    f = fuse(a, b)
    assert f.shape == (3, 10)
    assert torch.allclose(f.norm(dim=1), torch.ones(3), atol=1e-5)
