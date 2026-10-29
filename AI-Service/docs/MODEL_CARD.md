# Biometric-Pet-Identification — Model Card

**Status:** Template (to be filled as models are trained) · Format: Mitchell et al. *Model Cards*.

## Model details
- **Name:** Biometric-Pet-Identification dog re-ID (nose-print + face fusion) — v0 (planned).
- **Type:** Metric-learning embedding model + open-set thresholding head.
- **Architecture:** YOLOv8 detector → backbone (EfficientNet-B0/B2 or ResNeSt) → ArcFace/MagFace head →
  fused embedding → FAISS search → calibrated score. *(Final choices recorded in ADRs + filled here at M2/M4.)*
- **Inputs:** dog face & nose crops from phone photo/video. **Outputs:** identity embedding; at search time,
  ranked candidates + calibrated match probability + open-set verdict.

## Intended use
- **In scope:** assisting reunion of lost dogs and tracing strays to owners; shelter/vet intake matching.
- **Out of scope / prohibited:** any human identification; sole basis for euthanasia/ownership disputes;
  surveillance. Biometric-Pet-Identification output is **decision support**, not proof of ownership.

## Factors
Breed, coat colour/pattern, age, capture quality, lighting, pose. Thai soi/mixed breeds are an explicit
fairness factor (see DATA.md biases).

## Metrics (to be reported — see EVALUATION.md)
Rank-1, Rank-5, mAP (closed-set) · EER, FMR@recall (open-set) · top-K recall (UX) · ECE (calibration) ·
latency on RTX 3060.

## Evaluation data
Held-out **individuals** from PetFace (+ unseen set for verification); the Thai bootstrap set for
real-capture stress testing.

## Ethical considerations
- A false "no match" delays reunion; a false match exposes the wrong owner's contact → open-set FMR is
  the safety-critical metric, gated low.
- Owner contact is PDPA-sensitive; revealed only on a confident match.

## Caveats & recommendations
Cold-start: matching needs a populated gallery (mitigated by the 2026 registration wave). Cats unsupported
in v1. Models trained on research-only datasets cannot ship commercially without re-licensing/retraining.
