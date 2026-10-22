# ADR 0002 — Biometric modality: nose-print + face fusion (dog-first)

- **Status:** Accepted
- **Date:** 2026-06-08

## Context
Options for individual dog ID: nose-print, face, full body, or fusion. Literature: dog nose-print reaches
~99.8% Rank-1 (DNNetV2); face+body fusion lifts top-K recall toward ~100% (MDPI 2023). Real captures of
strays are uncooperative — a single modality's frame is often poor.

## Decision
v1 uses **nose-print + face fusion** on **dogs first**. Nose-print is the most discriminative biometric;
the face branch adds robustness when the nose frame is low quality. Cats are deferred to phase 2
(less data, weaker nose-print evidence) via CatFaceNet.

## Consequences
- (+) Robust to poor single-modality captures; strong literature support.
- (+) A single YOLOv8 detector serves both nose and face crops.
- (−) Two detect+embed paths to maintain; an ablation (nose/face/fusion) is required to justify the cost (M4).
- (−) Cat users unsupported in v1 (documented non-goal).
