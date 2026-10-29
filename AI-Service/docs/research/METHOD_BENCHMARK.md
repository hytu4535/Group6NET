# Biometric-Pet-Identification — Method Benchmark & Selection ("the most accurate method")

**Status:** v1.0 · **Date:** 2026-06-09 · Refines [`../adr/0002-biometric-modality.md`](../adr/0002-biometric-modality.md).
Sources cited `[n]` per [`REFERENCES.md`](REFERENCES.md).

## 1. Reported accuracy ranking (individual ID)

| Rank | Method | Modality | Backbone + Loss | Reported accuracy | Realism | Src |
|------|--------|----------|-----------------|-------------------|---------|-----|
| 🥇 | **DNNetV2** | dog **nose-print** | **Tiny-ViT + MagFace-style loss** | **~99.8% Rank-1** | curated captures | [11] |
| 🥈 | **DNNet** | dog nose-print | Siamese CNN + self-attention | ~98.97% Rank-1 | curated | [10] |
| 🥉 | **Nose-to-ID (2025)** | dog nose-print **(video)** | **YOLOv8 detect + ResNeSt embed** | robust @ thousands of dogs | **real-world video** | [12] |
| 4 | MDPI face+body | dog & cat | CNN transfer + **fusion** | face 80% / **fusion 86.5%, top-10 ≈ 100%** | real lost-pet imgs | [8] |
| 5 | Cat-recognition cmp | cat face | **ConvNeXt/DenseNet + transfer** | best-in-class for cats | moderate | [17] |
| 6 | Lin & Kuo | cat face | CNN + SVM | 94.1% | small set | [16] |
| ref | ArcFace | (general face) | ResNet-100 + angular margin | SOTA on face benchmarks | — | [19] |

**Headline:** the **single most accurate reported method is DNNetV2 — dog nose-print, Tiny-ViT backbone +
MagFace-style loss, ~99.8% Rank-1** [11]. The most *deployment-realistic* high-accuracy method is
**Nose-to-ID** (automated from phone video) [12]. Fusion adds robustness/top-K [8].

## 2. Why these win (not just the number)
- **Nose-print > face for dogs:** higher ceiling (papillary ridges are fingerprint-unique) [9,15].
- **MagFace-style loss > plain ArcFace here:** magnitude encodes **quality**, pushing noisy/low-quality
  captures away from class centres — exactly the stray-photo regime, and it hands Biometric-Pet-Identification a *free* quality
  signal for stage [0] [20]. DNNetV2's top result uses this family [11].
- **ViT/ResNeSt backbones** edge CNNs on this task at small param cost (Tiny-ViT is 6 GB-friendly) [11,12].
- **Fusion** (nose ⊕ face) lifts top-K toward ~100% and covers poor-nose frames [8].

## 3. Honest caveats (so we don't over-trust 99.8%)
- 99.8% is **dataset-dependent** and on relatively cooperative captures; **real strays are messier** →
  Nose-to-ID's video frame-selection + Biometric-Pet-Identification's quality gate are what protect accuracy in the field [12,27].
- **Open-set ≠ Rank-1:** none of the top numbers report false-match-rate on *unenrolled* animals; Biometric-Pet-Identification
  must still gate on FMR (ADR 0003) — a high Rank-1 model can still expose the wrong owner.
- Numbers come from web-sourced summaries; **re-verify on Biometric-Pet-Identification's own by-individual splits** (EVALUATION.md).

## 4. ✅ Selected method for Biometric-Pet-Identification v1 (most-accurate ∧ 6 GB-feasible)

> **Synthesis of DNNetV2 (accuracy) + Nose-to-ID (real-world pipeline) + MDPI fusion (robustness).**

```
[0] Capture+QC : video → head-pose → most-frontal frame (Nose-to-ID); MagFace magnitude as quality cue
[1] Detect     : YOLOv8 → dog vs cat, FACE box, NOSE box           (Nose-to-ID; Ultralytics YOLOv8)
[2] Align/Crop : normalise nose & face crops
[3] Embed      : NOSE branch  = Tiny-ViT / ResNeSt + MagFace loss   ← the 99.8% recipe [11,12]
                 FACE branch  = EfficientNet-B0/B2 (ConvNeXt for cats, phase 2 [17])
                 FUSE → identity descriptor                          (MDPI fusion [8])
[4] Match      : FAISS cosine NN over gallery → top-K
[5] Calibrate  : score→prob + open-set abstain, FMR-gated           (ADR 0003)
[6] Serve      : FastAPI + Streamlit, offline
```

**Pragmatic build order (don't start at the hardest model):**
1. **M2 baseline:** EfficientNet-B0 + **ArcFace** on nose crops — stable, easy to train, get a number.
2. **M2→M4 upgrade:** swap loss to **MagFace**, backbone to **Tiny-ViT/ResNeSt** → chase the 99.8% recipe.
3. **M4 fusion:** add face branch; ablate nose / face / fusion to *prove* fusion earns its complexity.

This sequence reaches the most-accurate method by upgrades, each measured against the last — matching the
anti-vibe-coding, verify-every-step workflow.
