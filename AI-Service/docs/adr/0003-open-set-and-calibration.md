# ADR 0003 — Open-set matching with calibrated confidence

- **Status:** Accepted
- **Date:** 2026-06-08

## Context
A found stray may be an animal that was **never enrolled**. A closed-set classifier (always returns a
"best" match) would confidently hand back the *wrong* owner's contact — a real-world harm. Re-ID papers
often optimise Rank-1 and under-report this.

## Decision
Treat matching as **open-set**: convert FAISS similarity to a **calibrated probability** (temperature/Platt
scaling) and apply an **abstain band** → verdict ∈ {match, possible, no confident match}. The operating
threshold is chosen to satisfy a **false-match-rate (FMR) ceiling first**, then maximise top-K recall.
Owner contact is revealed only on a confident match.

## Consequences
- (+) Safety-aligned: avoids confidently exposing the wrong owner.
- (+) Builds on standard calibration/conformal methods.
- (−) Some true matches fall into "possible" (lower apparent recall) — an accepted safety trade-off.
- (−) Requires a dedicated **unseen-individual** eval set (see DATA.md / EVALUATION.md).
