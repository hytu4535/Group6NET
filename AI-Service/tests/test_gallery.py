import numpy as np

from petsy.gallery import Gallery


def test_enroll_and_len():
    g = Gallery(dim=4)
    g.enroll(np.eye(4, dtype=np.float32), ids=[10, 11, 12, 13])
    assert len(g) == 4
    assert g.backend in ("faiss", "numpy")


def test_search_returns_self_as_top1():
    g = Gallery(dim=3)
    emb = np.array([[1, 0, 0], [0, 1, 0], [0, 0, 1]], dtype=np.float32)
    g.enroll(emb, ids=[1, 2, 3], meta=[{"owner": f"o{i}"} for i in (1, 2, 3)])
    q = np.array([[0.9, 0.1, 0.0]], dtype=np.float32)   # closest to id 1
    ids, sims = g.search(q, k=2)
    assert ids[0, 0] == 1
    assert sims[0, 0] >= sims[0, 1]


def test_meta_aligned():
    g = Gallery(dim=2)
    g.enroll(np.array([[1, 0]], dtype=np.float32), ids=[7], meta=[{"owner": "alice"}])
    assert g.meta[0]["owner"] == "alice"
