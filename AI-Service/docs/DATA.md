# Biometric-Pet-Identification — Data Documentation (Datasheet)

**Status:** Draft v1.0 · **Last updated:** 2026-06-08
Format follows *Datasheets for Datasets* (Gebru et al.) principles, scoped to Biometric-Pet-Identification.

## 1. Data strategy ladder (risk-managed)
1. **Build & benchmark now (open access)** — public datasets below.
2. **Bootstrap realism** — a small self-collected Thai phone-capture set (own/friends' pets) to test
   real-world capture quality (the failure mode that sank Google's DR deployment in Thailand).
3. **Real Thai data (partnership-gated, designed-for)** — BMA registry, Thai National Pet Registry,
   vet clinics, shelters, insurers. Requires consent + data-sharing agreements; **not** needed for v1.

## 2. Open datasets (primary build set)
| Dataset                                         | What                                                                                        | Role in Biometric-Pet-Identification                              | Notes                                    |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------- | ------------------------------------------ | ---------------------------------------- |
| **PetFace**                                     | 257,484 individuals, 13 animal families, 319 breeds; re-ID + unseen-verification benchmarks | Primary embedding training + open-set eval | Largest individual-level animal-face set |
| **Dog nose-print sets** (DNNet/DNNetV2 lineage) | Dog muzzle/nose-print images per individual                                                 | Nose-print branch                          | Confirm license per source               |
| **Flickr-dog**                                  | 374 imgs / 42 individuals                                                                   | Small dog-id building block                | Tiny — for sanity, not training scale    |
| **Stanford Dogs**                               | 20,580 imgs / 120 breeds                                                                    | Species/breed + detector pretraining       | Breed labels, not individual-id          |
| **Columbia Dogs**                               | Dog images w/ landmarks                                                                     | Face alignment / landmarks                 |                                          |
| **Cat facial-landmark set** (48 landmarks)      | Cat faces + landmarks                                                                       | **Phase-2** cats                           | Reserved                                 |

> ⚠️ **Licenses must be verified before training.** Each dataset's terms (research-only vs commercial)
> directly limit how a future Biometric-Pet-Identification product can use a model trained on it. Track per-dataset license in
> `data/LICENSES.md` (created at M0).

## 3. Splits & leakage policy
- Split **by individual**, never by image — the same animal must not appear in both train and test
  (otherwise re-ID metrics are inflated).
- Reserve a set of **unseen individuals** exclusively for **open-set / verification** evaluation.
- Seeded, deterministic splits committed as manifests (not the images).

## 4. Preprocessing
Detect → crop (face, nose) → align → resize → normalise. Augmentation: geometric (rotate/flip/scale),
photometric (brightness/contrast), and ultrasound-of-pets-irrelevant noise excluded. No identity-altering
augmentation.

## 5. Known biases & gaps
- Breed/coat-colour imbalance in public sets → may underperform on breeds common in Thailand
  (soi dogs / mixed breeds). Mitigation: the Thai bootstrap set + breed-stratified eval.
- Nose-print datasets skew to cooperative/clear captures; real strays won't be cooperative → the quality
  gate and video frame-selection matter.

## 6. Storage & governance
- Raw data under `data/` (gitignored). Never committed, never uploaded to cloud.
- Owner contact (future) is PDPA personal data — separate store, minimised, consented
  (see `PRIVACY_PDPA.md`).

## 7. Provenance / citations
See `docs/research/02-prior-art.md` for the papers and dataset sources behind each entry.
