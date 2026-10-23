# ADR 0005 — Data strategy: public-first, partnership-later

- **Status:** Accepted
- **Date:** 2026-06-08

## Context
Real Thai pet-registry data (BMA / Thai National Pet Registry / shelters) is partnership- and consent-gated
and not available to a solo developer up front. But the methodology can be built and validated on open data.

## Decision
Adopt a **three-rung ladder**: (1) build + benchmark on open datasets (PetFace, nose-print sets,
Flickr-dog, Stanford Dogs); (2) bootstrap realism with a small self-collected Thai phone-capture set;
(3) design a clean **registry adapter seam** so real Thai data can plug in later without rearchitecting.
v1 ships entirely on rungs 1–2.

## Consequences
- (+) Unblocks immediate progress; de-risks the partnership dependency.
- (+) The methodology *is* the deliverable; real-data validation is an additive later step.
- (−) Public datasets under-represent Thai soi/mixed breeds → breed-stratified eval + the bootstrap set
  required to surface bias.
- (−) Research-only licenses limit commercial use of models trained on them (tracked in `data/LICENSES.md`).
