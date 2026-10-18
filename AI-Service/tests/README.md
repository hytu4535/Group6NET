# tests/ — test suite (TDD)

Mirrors `src/` module-for-module. Each pipeline stage gets unit tests; the full enroll→search loop gets an
integration test. Write the test **before** the implementation (red → green → refactor).

Priorities once code starts (M1+):
- `capture/` — quality gate accept/reject on deliberately bad inputs (blur, occlusion, wrong species).
- `match/` — enroll/query correctness; open-set "no match" path.
- `calibrate/` — score→probability monotonicity; threshold behaviour.

Run: `pytest` (after deps are installed at M0).
