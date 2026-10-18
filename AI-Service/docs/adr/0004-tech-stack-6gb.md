# ADR 0004 — Tech stack for the 6 GB / offline constraint

- **Status:** Accepted
- **Date:** 2026-06-08

## Context
Target hardware is a single **RTX 3060 6 GB**, offline-first. The stack must train and serve within that
budget using proven tooling (PyTorch + AMP, FastAPI + Streamlit, conformal calibration).

## Decision
- **Framework:** PyTorch (CUDA + **AMP**), `timm` backbones (**EfficientNet-B0/B2**, ResNeSt).
- **Detection:** Ultralytics **YOLOv8** (already in the developer's toolkit).
- **Retrieval:** **FAISS-CPU** (sufficient at demo gallery scale; GPU swap available).
- **Serving:** **FastAPI + Streamlit**, offline-capable (mirrors Z+).
- **Memory tactics:** gradient accumulation, AMP, backbone ≤ B2, modest input resolution.

## Consequences
- (+) Fits 6 GB; minimal new tooling; fast path to a running system.
- (+) Reproducible, local-only (no cloud) — aligns with privacy rules.
- (−) Caps model size — very large backbones / heavy video models are out (acceptable for v1).
- (−) FAISS-CPU latency grows with gallery size — revisit at registry scale.
