# Research — Prior art: nose-print & pet-face re-ID

## Nose-print biometrics (dogs)
- **DNNet** — Siamese CNN + self-attention; **~98.97% Rank-1** (cross-validation). Smartphone images,
  no special scanner. (IEEE Access)
- **DNNetV2** — Tiny-ViT + MagFace-based loss; **~99.8% Rank-1**. Unified framework also includes
  **CatFaceNet** for cat faces (our phase-2 path).
- **Nose-to-ID (2025)** — fully automated from user video: head-pose picks the most frontal frame →
  **YOLOv8** nose detection → multi-resolution ResNeSt embeddings → fused descriptor across thousands of
  dogs. **This is the template for Biometric-Pet-Identification's stages [0]→[3].**

## Pet-face re-ID & datasets
- **PetFace** — 257,484 individuals, 13 families, 319 breeds; benchmarks for seen re-ID **and unseen
  verification** (the open-set case we care about). Primary training/eval set.
- **MDPI 2023 — "Pet Identification Using Face and Body"** — 7-stage pipeline; species class. 98.18%;
  face id 80%, body 81%, **face+body fusion 86.5%, top-10 ≈ 100%**. Motivates our fusion choice.
- Building blocks: **Flickr-dog** (374 imgs/42 ids), **Stanford Dogs** (120 breeds), **Columbia Dogs**,
  a **cat facial-landmark** set (48 landmarks).

## Deployment cautionary tale (why the quality gate is first-class)
- Google Health's **diabetic-retinopathy** deep-learning screening in **Thai clinics** (2018–2019):
  **21% of images rejected as ungradable** (lighting), slow connectivity, workflow bottlenecks — strong
  lab accuracy ≠ field performance. Biometric-Pet-Identification answers this with a first-class **quality gate** + offline operation.

## Implications for Biometric-Pet-Identification
1. Nose-print + face fusion is well-supported (ADR 0002).
2. YOLOv8 → embedding → vector search is a proven, 6 GB-friendly recipe (ADR 0004) and reuses existing skills.
3. Open-set/unseen verification must be evaluated explicitly (ADR 0003, EVALUATION.md).
4. Capture-quality handling is make-or-break in the field (RISK R2).

## Sources
DNNet (IEEE Xplore 9385064); DNNetV2 + CatFaceNet; Nose-to-ID (Guilan CSE, 2025); PetFace (arXiv);
MDPI Information 2023 14(5):278; Flickr-dog / Stanford Dogs / Columbia Dogs; Google DR Thailand
(Lancet Digital Health 2022; Newsweek; arXiv 2012.01165).

*(Captured 2026-06-08; confirm dataset licenses before training — see DATA.md.)*
