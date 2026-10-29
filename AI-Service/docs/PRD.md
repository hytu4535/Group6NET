# Biometric-Pet-Identification — Product Requirements Document (PRD)

**Status:** Draft v1.0 · **Owner:** Grechka67 · **Last updated:** 2026-06-08

## 1. Problem
Millions of pets go missing yearly; microchips — the incumbent ID — require a proprietary scanner (vets/
shelters only), are invasive, and aren't trackers. A member of the public who finds a stray has no way to
identify it. Meanwhile **Bangkok mandates pet registration + microchipping from 10 Jan 2026**, creating both
a registration wave and an unmet need for an accessible lookup tool.

## 2. Vision
"Snap a photo of any dog or cat — find out who it belongs to." A smartphone biometric (nose-print + face)
that anyone can use to reunite lost pets and trace strays, complementing — not replacing — microchips.

## 3. Goals & non-goals
**Goals (v1):**
- G1. Re-identify an enrolled **dog** from a phone photo or short video with high top-K recall.
- G2. Correctly answer **"no confident match"** for unenrolled animals (open-set safety).
- G3. Run end-to-end **locally on an RTX 3060 6 GB**.
- G4. Ship a usable **enroll → search demo** (Streamlit + FastAPI).
- G5. Be **registry-ready** — a clean integration path for BMA / shelters / vets.

**Non-goals (v1):** GPS/live tracking · real owner-PII at scale · production registry integration ·
cats (phase 2) · native mobile app (responsive web first) · human identification (forbidden, ever).

## 4. Users & jobs-to-be-done
| Persona | Job |
|---------|-----|
| **Owner** | Enroll my pet so it can be found if lost. |
| **Finder** (member of public) | Photograph a stray and find its owner — no special device. |
| **Shelter / vet staff** | Bulk-enroll at intake/chip time; check incoming strays against the registry. |
| **Pet insurer** (later) | Verify a claimed animal's identity (reduce false claims — a cited real driver). |

## 5. User flows
1. **Enroll:** capture (photo/video) → quality gate → detect face+nose → embed → store in gallery with
   owner contact. Output: confirmation + the captured biometric preview.
2. **Search:** capture probe → quality gate → embed → FAISS top-K → calibrated verdict
   (*match* / *possible* / *no confident match*) → owner contact revealed only on a confident match.

## 6. Functional requirements
- FR1 Accept image and ≤10 s video; auto-select the best frontal frame from video.
- FR2 Reject ungradable inputs with an actionable reason (blur / occluded / no animal / wrong species).
- FR3 Enroll & search dogs; persist gallery to disk.
- FR4 Return ranked candidates with a calibrated confidence and an explicit abstain option.
- FR5 Run fully offline.

## 7. Non-functional requirements
- NFR1 Single-image inference < ~2 s on RTX 3060.
- NFR2 Reproducible training (seeded, configs versioned).
- NFR3 PDPA-aligned handling of owner contact data (see `PRIVACY_PDPA.md`).
- NFR4 No cloud dependency for core inference.

## 8. Success criteria (measurable — see EVALUATION.md)
- S1 Correct match in **top-10 on a held-out gallery** (target ≈ literature's ~100% small-gallery).
- S2 **Rank-1 ≥ 0.85** on PetFace-style dog held-out IDs (stretch; baseline-dependent).
- S3 **Open-set:** false-match rate at the operating threshold ≤ target (set in EVALUATION).
- S4 The full enroll→find demo runs on the 3060 within NFR1 latency.

## 9. Assumptions & dependencies
Open datasets remain accessible (PetFace, Flickr-dog, nose-print sets). Real Thai data requires later
partnership (BMA / Thai National Pet Registry / shelters) — designed-for, not required for v1.

## 10. Open questions
Build mode (hybrid recommended) · final cat timing · which backbone wins (decided empirically in M2).
