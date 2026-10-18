# Biometric-Pet-Identification — Systematic Literature Review

**Scope:** biometric pet (dog/cat) individual re-identification for lost-pet reunion, and its enabling
methods (metric learning, open-set recognition, efficient models) + deployment evidence.
**Method:** multi-query web search across scholarly domains (arXiv, IEEE, CVF, Nature, ScienceDirect,
Springer, PMC), 2026-06-08/09. Citation style: Vancouver-ish inline `[n]`; full list in
[`REFERENCES.md`](REFERENCES.md). Targets influential/high-citation work first.
**Status:** v1.0 — living document; promote durable conclusions to ADRs.

---

## 1. Screening summary (PRISMA-style narrative)
Searches across ~14 query strings surfaced ~80 candidate records. After title/abstract screening for
relevance to *individual* animal ID, metric learning, open-set, efficient models, and deployment, **~30
sources** were retained as the evidence base below. Excluded: pure breed/species classification with no
individual-ID angle, and human-only face work not transferable to the pipeline. Surveys were preferred as
anchors; seminal method papers (ArcFace, FaceNet, MagFace) retained for foundations.

> A PRISMA flow figure is a TODO (the skill's figure generator/`scientific-schematics` is not installed
> here). Insert when tooling is available; the counts above are the textual equivalent.

---

## 2. Theme A — Pet & animal individual re-identification

**Field state.** Animal re-ID has matured from species-specific pattern matching (tiger stripes, seal
rings) to **species-agnostic deep descriptors**; a 2025 *Information Fusion* survey reviews 41 works
(2020–2025) and proposes a taxonomy spanning deep representation learning, transformers, multimodal and
vision-language methods [1]. Large benchmarks now exist (**WildlifeReID-10k**, 10k individuals [2];
**MetaWild** multimodal [3]) and "**toward re-identifying any animal**" (NeurIPS 2024) pushes universal
models [4]. Methodologically, **person re-ID** is the mature parent field Biometric-Pet-Identification borrows from — Zheng et
al.'s "Past, Present and Future" [5] and the 230+-paper challenge retrospective [6] catalogue the exact
problems Biometric-Pet-Identification faces (pose, occlusion, illumination, and the **closed- vs open-world gap**).

**Pet-specific.** The directly load-bearing precedents:
- **PetFace** — 257,484 individuals across 13 families; provides both seen re-ID and **unseen
  verification** benchmarks (the open-set case Biometric-Pet-Identification cares about) [7]. → primary training/eval set.
- **"Deep Learning Pet Identification Using Face and Body"** (MDPI 2023) — 7-stage pipeline; species
  class. 98.18%, face-ID 80%, body-ID 81%, **face+body fusion 86.5%, top-10 ≈ 100%** [8]. → motivates
  Biometric-Pet-Identification's **fusion** choice (ADR 0002) and the **top-K recall** UX metric.

**Implication.** Fusion + an explicitly evaluated open-set protocol are evidence-backed defaults.

## 3. Theme B — Dog nose-print biometrics (the core modality)

The nose-print is biologically unique (papillary ridges), functioning as a canine fingerprint [9,15].
Deep methods are strong and smartphone-friendly:
- **DNNet** — Siamese CNN + self-attention; **~98.97% Rank-1** [10].
- **DNNetV2 + CatFaceNet** — Tiny-ViT + MagFace-based loss; **~99.8% Rank-1**; unified dog-nose + cat-face
  framework (Biometric-Pet-Identification's phase-2 cat path) [11].
- **Nose-to-ID (2025)** — fully automated from user **video**: head-pose frame selection → **YOLOv8**
  nose detection → ResNeSt embeddings → fused descriptor over thousands of dogs [12]. → the template for
  Biometric-Pet-Identification stages [0]→[3].
- Competition + shape-feature work — a Kaggle/CVPR-workshop "competitive method for dog nose-print
  re-ID" [13] and a scale-shape/spatial-feature approach [14] confirm the modality is actively benchmarked.

**Implication.** Nose-print is the highest-yield modality and the recipe (YOLOv8 → embedding → search)
matches existing developer skills.

## 4. Theme C — Cat face recognition (phase 2)

Cats are harder (less data, weaker nose-print evidence) but feasible: Lin & Kuo's foundational CNN+SVM
reached **94.1%** on 1,500 images / 150 cats [16]; a 2024 comparison finds **ConvNeXt/DenseNet with
transfer learning beat Siamese/fine-tuning** for individual cats [17]; **CatFaceNet** [11] and a Kaggle
"Cat Individual Images" set provide a path. → ADR 0002 defers cats to phase 2 with this evidence.

## 5. Theme D — Metric learning (how embeddings are trained)

Biometric-Pet-Identification's embedding head sits in a well-charted lineage:
- **FaceNet** (CVPR 2015) — 128-D embedding on a hypersphere via **triplet loss**; introduced online
  semi-hard mining; the foundational deep-metric approach [18].
- **ArcFace** (CVPR 2019, ~6,982 citations) — **additive angular margin**; geodesic interpretation;
  near-zero overhead; the de-facto strong baseline [19]. → Biometric-Pet-Identification's default head (ADR 0002).
- **MagFace** (CVPR 2021 oral) — feature **magnitude encodes quality**; pushes hard/low-quality samples
  from class centres; gives a free quality signal useful for Biometric-Pet-Identification's quality gate [20]. DNNetV2 already
  uses a MagFace-style loss [11].
- Surveys catalogue triplet/quadruplet/N-pair/lifted-structured variants and the **triplet-mining**
  difficulty [21]. → start with ArcFace (stable softmax-style training), consider MagFace for the
  quality synergy.

## 6. Theme E — Open-set recognition & calibration (the safety core)

A found stray may be **unenrolled** → the system must *reject*, not force a wrong match. OSR, formalised
by Scheirer et al. and surveyed in "Managing the Unknown" [22] and the unified anomaly/novelty/OOD survey
[23], offers threshold-based, feature-space, and generative/prototype strategies, plus uncertainty
quantification. Person-re-ID surveys note the same closed→open-world shift as the field's frontier [5,6].
**Implication (ADR 0003):** treat matching as open-set; calibrate similarity → probability; gate on
**false-match-rate** before optimising recall. The `awesome_OpenSetRecognition_list` [24] is a tracking
resource.

## 7. Theme F — Efficient models for the 6 GB / offline constraint

Two complementary levers: **native lightweight designs** (MobileNet depthwise-separable; EfficientNet
compound scaling; SqueezeNet; ShuffleNet) and **compression** (pruning, quantization, distillation) —
quantization can cut size ~75% at ~1.5% accuracy cost [25,26]. Caveat: FLOP/param counts **mis-predict
latency** (memory-access cost matters) [25]; EfficientNet can be param-heavy for very low-end devices
[26]. → ADR 0004 picks EfficientNet-B0/B2 / ResNeSt with AMP; benchmark actual latency, not FLOPs.

## 8. Theme G — Deployment in low-resource / Thai settings (the cautionary tale)

Google Health's diabetic-retinopathy deep-learning screening in **Thai clinics** (Beede et al., 2020;
Lancet Digital Health 2022) is the field's canonical warning: **~21% of images rejected as ungradable**
(lighting), slow connectivity, workflow bottlenecks — strong lab accuracy ≠ field performance [27,28].
→ Biometric-Pet-Identification makes the **quality gate a first-class stage [0]** and runs **offline** (ADR 0004; RISK R2).

---

## 9. Synthesis & gaps Biometric-Pet-Identification addresses
1. **Consensus:** nose-print + face fusion, ArcFace/MagFace embeddings, FAISS-style retrieval, and
   YOLOv8 detection are the proven recipe [8,11,12,19,20].
2. **Under-served gap 1 — open-set rigor:** much pet-ID work reports Rank-1 but under-reports open-set
   false matches; Biometric-Pet-Identification elevates FMR to the primary safety metric [22,23].
3. **Under-served gap 2 — Thai-population realism:** public sets under-represent Thai soi/mixed breeds;
   none target the Jan-2026 BMA registration context. Biometric-Pet-Identification's bootstrap set + breed-stratified eval
   addresses this [8, market docs].
4. **Under-served gap 3 — field capture quality:** the Google-DR lesson is rarely engineered for in pet
   apps; Biometric-Pet-Identification's quality gate + video frame selection is a deliberate answer [12,27].

## 10. Quality note
Sources skew to peer-reviewed venues (CVPR, NeurIPS, Information Fusion, Lancet Digital Health, IEEE) and
high-citation foundations (ArcFace, FaceNet). Some pet-specific results are single-study and dataset-
dependent — treat absolute accuracies as indicative, not guaranteed; Biometric-Pet-Identification re-validates on its own splits
(EVALUATION.md). Market figures are vendor-report estimates with wide variance (see business docs).
