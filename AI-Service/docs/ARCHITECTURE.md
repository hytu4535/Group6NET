# Biometric-Pet-Identification — Architecture

**Status:** Draft v1.0 · **Last updated:** 2026-06-08

## 1. System overview
Biometric-Pet-Identification is a **biometric re-identification pipeline** with two entry points (enroll, search) over a shared
embedding + vector-search core. Designed for offline single-GPU operation, with a clean seam for a future
registry backend.

## 2. Pipeline (stage by stage)

```
phone photo / ≤10s video
        │
        ▼
[0] CAPTURE & QUALITY GATE
    - video → head-pose scoring → pick best frontal frame (Nose-to-ID approach)
    - reject blur / occlusion / no-animal / wrong-species with a reason
        │
        ▼
[1] DETECT  (YOLOv8)                      ← Ultralytics YOLOv8
    - species: dog vs cat
    - boxes: FACE, NOSE
        │
        ▼
[2] ALIGN / CROP
    - normalise nose & face crops to fixed size
        │
        ▼
[3] EMBED  (metric-learning model)        ← NEW skill: metric learning
    - backbone: EfficientNet-B0/B2 or ResNeSt (timm)
    - head/loss: ArcFace / MagFace (or triplet)
    - produce NOSE embedding + FACE embedding → FUSE → identity descriptor
        │
        ├──(enroll)──► store {descriptor, owner_contact} in GALLERY
        │
        ▼ (search)
[4] MATCH  (FAISS)
    - nearest-neighbour search over gallery → top-K candidates + similarities
        │
        ▼
[5] CALIBRATE + OPEN-SET                   ← calibrated confidence (temperature / Platt)
    - similarity → calibrated match probability (temperature / Platt)
    - abstain band → verdict: match | possible | no confident match
        │
        ▼
[6] SERVE  (FastAPI + Streamlit)          ← offline-capable serving
    - /enroll, /search endpoints; Streamlit UI; offline-capable
```

## 3. Why these choices (links to ADRs)
- **Nose-print + face fusion** over nose-only: robustness when one modality's frame is poor — see
  [`adr/0002`](adr/0002-biometric-modality.md).
- **Open-set + calibration first-class** (not just Rank-1): a found stray may be unenrolled — see
  [`adr/0003`](adr/0003-open-set-and-calibration.md).
- **Lightweight backbone + AMP + FAISS-CPU** for the 6 GB budget — see
  [`adr/0004`](adr/0004-tech-stack-6gb.md).

## 4. Components & modules (planned `src/` layout)
```
src/
  capture/        # quality gate, frame selection
  detect/         # YOLOv8 wrappers (species, face, nose)
  embed/          # backbone + metric-learning head, fusion
  match/          # FAISS gallery: build, enroll, query
  calibrate/      # score → probability, open-set thresholding
  serve/          # FastAPI app + Streamlit UI
  common/         # config, seeding, logging, types (uses CONTEXT.md terms)
```

## 5. Data flow & storage
- **Gallery:** FAISS index + a sidecar table mapping vector-id → {individual-id, owner_contact}.
  Owner contact is PDPA-sensitive → stored separately, encrypted at rest in any non-demo build.
- **Models:** versioned `.pt` checkpoints (gitignored); configs versioned in-repo.

## 6. Hardware budget (RTX 3060 6 GB)
- Training: batch sizing via gradient accumulation; AMP (fp16); backbone ≤ EfficientNet-B2.
- Inference: detection + embedding fit comfortably in 6 GB; FAISS search is CPU.

## 7. Future / integration seam
A `registry` adapter interface (enroll/lookup) lets the on-disk gallery be swapped for a BMA/shelter
backend without touching stages [0]–[5]. Cats add a parallel detect+embed path (CatFaceNet) behind the
same match/calibrate/serve stages.

## 8. Quality attributes
Reproducibility (seeds, configs) · offline-first · privacy-by-design · testability (each stage unit-tested,
pipeline integration-tested — see `tests/`).
