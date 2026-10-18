import numpy as np

from petsy.capture import quality_gate


def _img(val, size=64):
    return np.full((3, size, size), val, dtype=np.float32)


def test_accepts_a_normal_textured_image():
    rng = np.random.default_rng(0)
    img = rng.random((3, 64, 64)).astype(np.float32)   # plenty of detail
    res = quality_gate(img)
    assert res.ok and res.reason == "ok"


def test_rejects_flat_image_as_blurry():
    res = quality_gate(_img(0.5))      # zero detail -> no sharpness
    assert not res.ok and res.reason == "too blurry"


def test_rejects_too_dark():
    rng = np.random.default_rng(1)
    img = rng.random((3, 64, 64)).astype(np.float32) * 0.02   # textured but near-black
    res = quality_gate(img)
    assert not res.ok and res.reason == "too dark"


def test_rejects_overexposed():
    rng = np.random.default_rng(2)
    img = 1.0 - rng.random((3, 64, 64)).astype(np.float32) * 0.02  # near-white
    res = quality_gate(img)
    assert not res.ok and res.reason == "overexposed"
