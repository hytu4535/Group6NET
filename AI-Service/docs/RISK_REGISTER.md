# Biometric-Pet-Identification — Risk Register

**Status:** Living document · **Last updated:** 2026-06-08
Severity = Likelihood × Impact. Owner = maintainer unless noted.

| ID | Risk | Sev | Mitigation | Status |
|----|------|-----|------------|--------|
| R1 | **Cold-start** — matching needs a populated gallery | High (product) | Ride Jan-2026 BMA registration wave; B2B to vets/shelters/insurers who already enroll; v1 *demonstrates* the loop on public data (no scale needed) | Open |
| R2 | **Real-world capture quality** (the Google-DR Thailand failure mode) | High | Quality gate is a **first-class stage [0]**; video head-pose frame selection; reject-and-reguide UX; Thai bootstrap capture set | Mitigating |
| R3 | **Open-set false match** → wrong owner's contact exposed | High | Calibrated probabilities + abstain band; FMR@recall gated low as the primary safety metric | Designed |
| R4 | **Dataset licenses** block commercial use | Med | Track per-dataset license at M0; keep a clean-licensed retraining path; research-only models stay research-only | Open |
| R5 | **Cats harder than dogs** (less data, weaker nose-print) | Med | Dog-first v1; cats phase 2 via CatFaceNet + cat-landmark set | Deferred |
| R6 | **6 GB VRAM** limits model size | Low | Light backbones (Eff-B0/B2), AMP, gradient accumulation, FAISS-CPU | Controlled |
| R7 | **PDPA / owner-PII** mishandling | Med | Data minimisation, consent, separated+encrypted contact store, no human ID (see PRIVACY_PDPA.md) | Designed |
| R8 | **Differentiation** vs scanner-only microchips | Low-Med | Nose-print biometric + open-set + Thai-law timing is a genuine wedge | Accepted |
| R9 | **Scope creep** (cats, mobile, registry all at once) | Med | Strict v1 non-goals in PRD; vertical-slice roadmap | Controlled |
| R10 | **Solo bandwidth** + learning curve (metric learning new) | Med | Hybrid build mode + `/tutor` on the ML core; reuse Z+ serving/calibration | Mitigating |
| R11 | **Funded competitor localises first** (Petnow enters Thailand) | Med-High | Move in the 2026 window; win Thai dataset + registry seat; out-localise not out-tech (see COMPETITIVE_LANDSCAPE) | Open |
| R12 | **Cold-start gallery liquidity** (business) | High | B2B partners pre-populate via mandated enrollment before consumer reliance (GO_TO_MARKET §3) | Mitigating |
| R13 | **Partnership/sales-cycle length** (shelters/insurers/registry slow) | Med | Parallel-track BD with tech build; target 1 LOI in 90 days | Open |
| R14 | **Dataset license blocks commercialisation** | Med | `licensing-architect` review; clean-licensed retraining path; `data/LICENSES.md` gate | Open |

## Review cadence
Re-score at the close of each milestone (M1–M6). New risks appended with the next free ID.
