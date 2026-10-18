# Biometric-Pet-Identification — Runbook

How to run what exists today, and how to move from the synthetic sanity to real GPU training.

## 0. What's working now (v0.2)
A complete, tested re-ID pipeline runs end-to-end (synthetic + real Oxford photos): metric-learning
(ArcFace **and** MagFace) → FAISS gallery → open-set calibrated matching → metrics, served via
FastAPI + Streamlit. Plus detector data-prep + train script (M1) and two-branch fusion (M4).

**Env:** `.venv` now has **torch 2.6.0+cu124 (GPU verified on RTX 3060)** + faiss + fastapi/streamlit.
**Verified:** GPU+AMP path runs (`device=cuda`), Rank-1 = 1.0, open-set accuracy = 1.0 (≈14 epochs),
**25/25 tests pass**, reproducible (seeded). Note: rank-1 hits 1.0 by ~8 epochs; the strict open-set
FMR threshold needs ~12-14 epochs on synthetic to fully separate.

## 1. Setup (already done once)
```bash
python -m venv .venv
./.venv/Scripts/python.exe -m pip install -U pip numpy pytest
./.venv/Scripts/python.exe -m pip install torch --index-url https://download.pytorch.org/whl/cpu
./.venv/Scripts/python.exe -m pip install faiss-cpu
```

## 2. Run the tests + sanity
```bash
./.venv/Scripts/python.exe -m pytest -q
./.venv/Scripts/python.exe scripts/sanity_check.py --head arcface --epochs 10
./.venv/Scripts/python.exe scripts/sanity_check.py --head magface --epochs 10
```

## 2b. Run the clickable demo (FastAPI + Streamlit)
```bash
# Streamlit UI (enroll/find synthetic pets; upload an image to see the quality gate)
./.venv/Scripts/python.exe -m streamlit run src/petsy/app.py

# OR the REST API
./.venv/Scripts/python.exe -m uvicorn petsy.api:app --app-dir src
#   GET  /health   GET /pets   POST /search?demo=known|unknown   POST /enroll (file,name,owner)
```
First launch trains a small model on synthetic data and caches it to `outputs/demo/`.
Verified live: enrolled pet → `match` (with owner); unknown pet → not falsely matched (open-set safety).

## 3. GPU (RTX 3060) — ✅ DONE
Torch is already the CUDA build (**torch 2.6.0+cu124**, `cuda_available=True`); `Config(device="auto")`
uses CUDA + AMP automatically. Still to add when training for real backbones/detector:
```bash
./.venv/Scripts/python.exe -m pip install timm ultralytics    # real backbones + YOLOv8
```

## 4. Plug in real data + the 99.8% recipe (the only manual steps)
0. **Get data** (`docs/DATA_SOURCES.md`):
   - `python scripts/download_data.py --oxford-pet`            # license-clean, no token
   - add Kaggle token → `python scripts/download_data.py --dog-noseprint`
   - flat set → `python scripts/organize_data.py ...`         # -> folder-per-identity
   - PetFace → fill the Google form (gated).
1. **Download datasets** (accept licenses, record in `data/LICENSES.md`):
   PetFace, a dog nose-print set, Stanford/Oxford. Place under `data/`.
2. **Implement the real loader** behind the same `(images, labels)` contract as
   `petsy.data.make_synthetic` (split BY INDIVIDUAL; hold out unknown ids for open-set).
3. **Switch to the selected method** (configs/default.yaml):
   - `backbone: timm:tiny_vit_5m_224` (nose) / `timm:efficientnet_b0` (face)
   - `head: magface`
4. **Train + evaluate** via `petsy.evaluate.run_pipeline`. Re-validate metrics on real splits.
5. **Detection:** replace `petsy/detect.py` stub with a fine-tuned YOLOv8 (nose + face boxes).
6. **Fusion:** add the face branch and ablate nose / face / fusion (M4).

## 4b. Train the detector (M1) — Oxford head boxes
```bash
python scripts/download_data.py --oxford-pet        # if not already
python scripts/prepare_oxford_yolo.py               # -> data/oxford_yolo/ (cat/dog + head box)
pip install ultralytics
python scripts/train_detector.py --epochs 50 --device 0   # GPU; outputs runs/pethead-detector/
```
Then wire `best.pt` into `petsy/detect.py` (replace the stub). Add a `nose` class once you have
nose-annotated data.

## 4c. Fusion (M4) — nose ⊕ face
`petsy/fusion.py` trains a two-branch embedder on paired crops:
```python
from petsy.fusion import train_fusion, extract_fused
model, _ = train_fusion(cfg, nose_x, face_x, y)      # paired crops from the detector
emb = extract_fused(model, nose_x, face_x, device)   # fused descriptor (2x embed_dim)
```
Run the **ablation** (nose-only vs face-only vs fusion) to justify the extra branch.

## 5. Module map (`src/petsy/`)
`config` · `data` (synthetic gen + dataset) · `capture` (quality gate) · `detect` (YOLOv8 stub) ·
`models` (backbones + ArcFace/MagFace + fusion) · `train` · `gallery` (FAISS/numpy) ·
`calibrate` (open-set) · `metrics` (Rank-k/mAP/EER/FMR) · `evaluate` (orchestration).

## 6. Known limitations (v0.1)
CPU-only until step 3; detection stubbed; single-modality (fusion is wired but face branch not trained);
synthetic data only until step 4. None are blockers — they're the labelled next steps.
