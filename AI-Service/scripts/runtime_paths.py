"""Environment-configurable paths shared by the FAISS API and gallery utilities."""
from __future__ import annotations

import os
from pathlib import Path

_CODE_ROOT = Path(__file__).resolve().parents[1]


def _configured_path(variable: str, default: Path, *, root: Path) -> Path:
    value = os.getenv(variable)
    path = Path(value).expanduser() if value else default
    if not path.is_absolute():
        path = root / path
    return path.resolve()


PROJECT_ROOT = _configured_path("PET_AI_ROOT", _CODE_ROOT, root=_CODE_ROOT)
MODEL_PATH = _configured_path(
    "PET_MODEL_PATH",
    PROJECT_ROOT / "models" / "pet-recognition-base.onnx",
    root=PROJECT_ROOT,
)
GALLERY_DIR = _configured_path(
    "PET_GALLERY_DIR",
    PROJECT_ROOT / "gallery",
    root=PROJECT_ROOT,
)
VECTOR_STORE_DIR = _configured_path(
    "PET_VECTOR_STORE_DIR",
    PROJECT_ROOT / "vector_store",
    root=PROJECT_ROOT,
)
INDEX_PATH = _configured_path(
    "PET_FAISS_INDEX_PATH",
    VECTOR_STORE_DIR / "pets.faiss",
    root=PROJECT_ROOT,
)
MAPPING_PATH = _configured_path(
    "PET_MAPPING_PATH",
    VECTOR_STORE_DIR / "mapping.json",
    root=PROJECT_ROOT,
)
