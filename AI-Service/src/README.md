# src/ — implementation (empty until M1)

Planned modules (see `docs/ARCHITECTURE.md` §4):

```
capture/   quality gate, video frame selection
detect/    YOLOv8 wrappers (species, face, nose)
embed/     backbone + metric-learning head, fusion
match/     FAISS gallery: build, enroll, query
calibrate/ score → probability, open-set thresholding
serve/     FastAPI app + Streamlit UI
common/    config, seeding, logging, shared types
```

No code yet — this project is in the **docs-first** phase. Implementation begins at **M1** (see ROADMAP).
Build via TDD: write the test in `tests/` first.
