# Biometric-Pet-Identification — Privacy & PDPA Compliance

**Status:** Draft v1.0 · **Last updated:** 2026-06-08
Reference: Thailand **Personal Data Protection Act B.E. 2562 (PDPA)**.

## 1. What data Biometric-Pet-Identification touches
| Data | Personal data? | Handling |
|------|----------------|----------|
| **Pet images / biometrics** (nose-print, face) | **No** (animal, not a person) | Core processing; not regulated as personal data. |
| **Owner contact** (name, phone, address) | **Yes — personal data** | Minimise, consent, secure, separate store. |
| **Finder photos** | May incidentally include people/plates | Strip/blur incidental humans; store only the animal crop. |
| **Location of capture** (future) | Potentially identifying | Out of v1 scope; gated behind consent if ever added. |

## 2. Principles applied
- **Lawful basis & consent:** owners explicitly consent at enrollment to storing contact data for the
  purpose of reunion. Consent is recorded, purpose-limited, and revocable.
- **Data minimisation:** store only what reunion requires; the model needs the *pet* biometric, not the
  owner's identity, to function — owner contact is revealed only on a confident match.
- **Purpose limitation:** data used solely for lost-pet reunion / registry matching. No secondary use,
  no sale, no human identification — ever.
- **Storage limitation:** retention policy + deletion on owner request (DSAR support).
- **Security:** owner-contact store separated from biometrics; encrypted at rest in any non-demo build;
  secrets never committed (`.gitignore` covers `.env`, keys).
- **Data subject rights:** access, rectification, erasure, withdrawal of consent.

## 3. v1 (demo) scope
The v1 demo uses **public pet datasets + synthetic/own owner contacts only** — no third-party personal
data. This keeps development PDPA-clean while the architecture bakes in the controls above for the real build.

## 4. Hard prohibition
Biometric-Pet-Identification must **never** be repurposed to identify humans. Any human faces incidentally captured are blurred/
discarded. This is a non-negotiable project rule (see `CLAUDE.md`).

## 5. Before any real deployment
- Data Protection Impact Assessment (DPIA).
- Data-sharing agreements with BMA / registry / shelters.
- Appointed data controller + (if thresholds met) DPO.
- Per-dataset license review (research-only datasets cannot back a commercial model).
