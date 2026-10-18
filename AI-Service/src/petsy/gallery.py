"""Enrolled-pet gallery with nearest-neighbour search.

Uses FAISS (inner-product on L2-normalised vectors = cosine) when available, else a
numpy brute-force fallback so the pipeline runs even without faiss installed. Same
contract either way. Stores owner metadata alongside each individual id.
"""
from __future__ import annotations

import numpy as np

try:
    import faiss  # type: ignore
    _HAS_FAISS = True
except Exception:
    _HAS_FAISS = False


def _normalize(x: np.ndarray) -> np.ndarray:
    n = np.linalg.norm(x, axis=1, keepdims=True)
    return x / np.clip(n, 1e-12, None)


class Gallery:
    def __init__(self, dim: int):
        self.dim = dim
        self._emb = np.empty((0, dim), dtype=np.float32)
        self.ids: list[int] = []
        self.meta: list[dict] = []
        self._index = faiss.IndexFlatIP(dim) if _HAS_FAISS else None

    @property
    def backend(self) -> str:
        return "faiss" if _HAS_FAISS else "numpy"

    def __len__(self) -> int:
        return len(self.ids)

    def enroll(self, embeddings: np.ndarray, ids, meta: list[dict] | None = None) -> None:
        emb = _normalize(np.ascontiguousarray(embeddings, dtype=np.float32))
        ids = list(ids)
        meta = meta or [{} for _ in ids]
        assert len(ids) == len(emb) == len(meta)
        self._emb = np.concatenate([self._emb, emb], axis=0)
        self.ids.extend(ids)
        self.meta.extend(meta)
        if self._index is not None:
            self._index.add(emb)

    def search(self, queries: np.ndarray, k: int = 5):
        """Return (topk_ids[N,k], topk_sims[N,k]) by cosine similarity."""
        q = _normalize(np.ascontiguousarray(queries, dtype=np.float32))
        k = min(k, len(self.ids))
        if k == 0:
            raise ValueError("gallery is empty")
        if self._index is not None:
            sims, idx = self._index.search(q, k)
        else:
            sims_full = q @ self._emb.T
            idx = np.argsort(-sims_full, axis=1)[:, :k]
            sims = np.take_along_axis(sims_full, idx, axis=1)
        ids_arr = np.asarray(self.ids)
        return ids_arr[idx], sims
