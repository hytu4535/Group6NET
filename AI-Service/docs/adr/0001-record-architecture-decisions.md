# ADR 0001 — Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-06-08

## Context
Biometric-Pet-Identification is decision-heavy (modality, losses, thresholds, data licensing) and may later be handed to clinical/
registry partners or revisited after a break. Decisions need durable, scannable rationale.

## Decision
We record every significant, hard-to-reverse decision as a numbered ADR in `docs/adr/`, using the format
Context / Decision / Status / Consequences. Research backing a decision lives in `docs/research/` and the
Obsidian `vault/`.

## Consequences
- (+) Future-you and any collaborator can see *why*, not just *what*.
- (+) Aligns with the `grill-with-docs` / `improve-codebase-architecture` skills.
- (−) Small ongoing writing cost — accepted.
