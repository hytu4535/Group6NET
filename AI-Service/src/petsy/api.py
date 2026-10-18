"""FastAPI backend — enroll & search endpoints over the Biometric-Pet-Identification pipeline.

Endpoints:
  GET  /health
  GET  /pets                       -> enrolled profiles
  POST /search   (file | ?demo=known|unknown)   -> verdict + candidates
  POST /enroll   (file, name, owner)            -> new enrollment

Run:  ./.venv/Scripts/python.exe -m uvicorn petsy.api:app --app-dir src --reload
"""
from __future__ import annotations

import io

import numpy as np
from fastapi import FastAPI, File, Form, HTTPException, Query, UploadFile

from .serve_state import DemoState

app = FastAPI(title="Biometric-Pet-Identification API", version="0.1.0")
_STATE: DemoState | None = None


def state() -> DemoState:
    global _STATE
    if _STATE is None:
        _STATE = DemoState.build(verbose=False)
    return _STATE


def _image_to_array(raw: bytes, size: int, channels: int) -> np.ndarray:
    from PIL import Image
    img = Image.open(io.BytesIO(raw)).convert("RGB" if channels == 3 else "L")
    img = img.resize((size, size))
    arr = np.asarray(img, dtype=np.float32) / 255.0
    if channels == 3:
        arr = np.transpose(arr, (2, 0, 1))      # HWC -> CHW
    else:
        arr = arr[None, ...]
    return arr


@app.get("/health")
def health():
    s = state()
    return {"status": "ok", "device": s.device, "backend": s.gallery.backend,
            "enrolled": len(s.profiles)}


@app.get("/pets")
def pets():
    s = state()
    return {"pets": [{"id": i, **p} for i, p in sorted(s.profiles.items())]}


@app.post("/search")
async def search(file: UploadFile | None = File(default=None),
                 demo: str | None = Query(default=None, pattern="^(known|unknown)$")):
    s = state()
    if demo is not None:
        img, _ = s.sample_query(known=(demo == "known"))
    elif file is not None:
        img = _image_to_array(await file.read(), s.cfg.img_size, s.cfg.channels)
    else:
        raise HTTPException(status_code=400,
                            detail="provide an image file or ?demo=known|unknown")
    return s.query_array(img)


@app.post("/enroll")
async def enroll(name: str = Form(...), owner: str = Form(...),
                 file: UploadFile = File(...)):
    s = state()
    img = _image_to_array(await file.read(), s.cfg.img_size, s.cfg.channels)
    return s.enroll_array(img, name=name, owner=owner)
