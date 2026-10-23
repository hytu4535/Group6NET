# Biometric-Pet-Identification — Evaluation Protocol

**Status:** Draft v1.0 · **Last updated:** 2026-06-08
Goal: make "does it work?" objective and reproducible (karpathy: verifiable goals).

## 1. Tasks evaluated
1. **Closed-set re-ID** — probe is a known enrolled individual. Metrics: **Rank-1, Rank-5, mAP**.
2. **Open-set verification** — probe may be unenrolled. Metrics: **ROC, EER, FMR@fixed-recall**.
3. **Lost-pet UX** — does the right pet appear in the shortlist? Metric: **top-K recall** (K=5, 10).
4. **Quality gate** — correct accept/reject. Metrics: reject precision/recall on a labelled bad-input set.
5. **Calibration** — are confidences trustworthy? Metric: **ECE** + reliability diagram.
6. **Systems** — single-image latency & VRAM on RTX 3060.

## 2. Protocol
- **Split by individual** (never by image). A held-out set of **unseen individuals** is used only for
  open-set/verification — they never appear in training.
- Fixed seeds; configs + split manifests committed. Each reported number is reproducible from a config.
- Report mean ± std over ≥3 seeds for trained components.

## 3. Operating point
The deployed threshold is chosen on a validation set to satisfy the **open-set FMR ceiling first**
(safety), then maximise top-K recall. Document the chosen threshold and the trade-off curve.

## 4. Targets (v1)
| Metric | Target | Rationale |
|--------|--------|-----------|
| Top-10 recall (small gallery) | ≈ 1.0 | Literature achieves ~100% in top-10 |
| Rank-1 (held-out dog IDs) | ≥ 0.85 (stretch) | Baseline-dependent; set firm after M2 |
| Open-set FMR @ 0.90 recall | ≤ TBD (set at M4) | Safety-critical — wrong owner exposure |
| ECE | ≤ 0.05 | Trustworthy confidence |
| Latency (1 image, 3060) | < ~2 s | NFR1 |

## 5. Baselines & sanity checks
- Reproduce a published PetFace re-ID number to confirm the training pipeline is correct *before*
  trusting our own results.
- Ablation: nose-only vs face-only vs **fusion** (expect fusion ≥ either alone).

## 6. Definition of done (v1 demo)
Enroll 20+ dogs from video → fresh photos return top-1 correct with calibrated confidence; a genuinely
unenrolled dog returns "no confident match" — all locally on the 3060 through the Streamlit UI. The full
metric panel is reported in the README.
