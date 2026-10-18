# CONTEXT — Ubiquitous Domain Language

> The shared vocabulary for Biometric-Pet-Identification. Use these exact terms in code, commits, docs, and conversation so the
> codebase stays AI- and human-navigable. (Pattern borrowed from DDD + the `grill-with-docs` skill.)

## Biometric / re-identification terms
| Term | Meaning in Biometric-Pet-Identification |
|------|------------------|
| **Individual** | One specific animal (not a breed or species). The thing we identify. |
| **Enrollment** | Registering an individual: capture → embedding → store in the gallery with owner contact. |
| **Probe** | An incoming query image/video of an animal we want to identify (e.g. a found stray). |
| **Gallery** | The store of enrolled embeddings searched against a probe. |
| **Embedding** | A fixed-length vector that represents an individual's identity. Close vectors ⇒ same animal. |
| **Re-identification (re-ID)** | Matching a probe to the correct enrolled individual (closed-set). |
| **Verification** | Deciding whether two captures are the *same* individual (often **unseen** during training). |
| **Open-set** | The realistic case: the probe may be an animal that was **never enrolled** → must answer "no match". |
| **Nose-print** | The unique pattern of beads/ridges on a dog's nose — a biometric like a fingerprint. |
| **Quality gate** | The stage that rejects blurred/occluded/wrong-pose/wrong-species frames before matching. |

## Metric terms
| Term | Meaning |
|------|---------|
| **Rank-1 / Rank-5** | Probability the correct individual is the top-1 / within top-5 ranked matches. |
| **mAP** | Mean average precision across queries (ranking quality). |
| **EER** | Equal Error Rate — the threshold where false-accept = false-reject. Lower is better. |
| **Top-K recall** | Probability the correct match appears in the top K results (the lost-pet UX metric). |
| **False-match rate (FMR)** | How often an unenrolled animal is wrongly matched to someone. Safety-critical. |
| **ECE** | Expected Calibration Error — are confidence scores trustworthy? |

## Product / Thailand terms
| Term | Meaning |
|------|---------|
| **BMA** | Bangkok Metropolitan Administration — issuer of the Jan 2026 pet-registration ordinance. |
| **Microchip** | RFID transponder under the skin; needs a proprietary scanner; the incumbent Biometric-Pet-Identification complements. |
| **Registry** | A database mapping individuals → owners (BMA / Thai National Pet Registry / shelters). |
| **PDPA** | Thailand's Personal Data Protection Act B.E. 2562 — governs owner contact data. |

## Engineering terms
| Term | Meaning |
|------|---------|
| **Vertical slice** | A milestone that is independently runnable/demoable end-to-end. |
| **Backbone** | The pretrained CNN/ViT feature extractor (EfficientNet-B0/B2, ResNeSt). |
| **Fusion** | Combining nose-print + face embeddings into one identity descriptor. |
