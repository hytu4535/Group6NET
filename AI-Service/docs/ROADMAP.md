# Biometric-Pet-Identification — Roadmap (Vertical Slices)

**Status:** Draft v1.0 · **Last updated:** 2026-06-08
Each milestone is **independently demoable** end-to-end. Close a milestone with `/code-review` + `/verify`.

## M0 — Setup & decisions  ✅ (mostly done 2026-06-09)
- ✅ Repo + venv; CPU torch + faiss + pytest installed; 10 agent skills installed in `.claude/agents/`.
- ✅ **v0.1 runnable pipeline on synthetic data** (ArcFace + MagFace → FAISS → open-set), 16/16 tests pass.
- ⬜ Confirm open decisions (build mode = hybrid; dog-only v1; goal emphasis).
- ⬜ Download PetFace + nose-print sets; write `data/LICENSES.md`; swap to CUDA torch + timm/ultralytics.
- **Done when:** real datasets present + license-tracked, GPU training runs. (Synthetic plumbing already proven.)

## M1 — Detect slice  🟡 (code-complete; needs ultralytics+GPU to train)
- ✅ Quality gate (`petsy/capture.py`); ✅ Oxford→YOLO data prep (`scripts/prepare_oxford_yolo.py`,
  cat/dog + head box); ✅ YOLOv8 train script (`scripts/train_detector.py`, guarded).
- ⬜ Run training (needs `pip install ultralytics` + CUDA); add a **nose** class from nose-annotated data;
  wire `best.pt` into `petsy/detect.py` (replace stub); video frontal-frame picker.
- **Demo:** upload image/video → boxes + accept/reject with reason.

## M2 — Embed slice
- Metric-learning model (ArcFace head on EfficientNet-B0 to start); train on PetFace; report Rank-1/mAP;
  reproduce a published baseline as a sanity check.
- **Demo:** embedding + closed-set Rank-1 on held-out IDs.

## M3 — Match slice
- FAISS gallery; enroll/search loop; top-K retrieval.
- **Demo:** enroll N dogs → query returns ranked matches.

## M4 — Fusion + open-set  🟡 (code-complete; needs real paired data)
- ✅ Open-set calibrated verdict + FMR gate (`petsy/calibrate.py`); ✅ EER/FMR metrics (`petsy/metrics.py`);
  ✅ two-branch nose⊕face fusion trainer (`petsy/fusion.py`, tested on synthetic).
- ⬜ Feed real paired (nose, face) crops from the detector; run the nose/face/fusion **ablation** to prove
  fusion earns its cost; report ECE on real data.
- **Demo:** confident match vs "no confident match" on unenrolled dog (working in v0.1 serve slice).

## M5 — Serve slice
- FastAPI (`/enroll`, `/search`) + Streamlit UI; offline-capable; full enroll→find on the 3060.
- **Demo:** the end-to-end product loop in the browser.

## M6 — Story & polish
- README metrics panel, demo clip, architecture write-up; framed for internships + a registry/shelter pitch.
- **Done when:** a stranger can read the README and run the demo.

## Phase 2 (post-v1)
Cats (CatFaceNet + cat-landmark set) · Thai phone-capture eval set · registry-integration design doc ·
mobile-native capture.

