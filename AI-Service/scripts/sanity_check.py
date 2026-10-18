"""End-to-end synthetic sanity check.

Proves the Biometric-Pet-Identification pipeline runs: synthetic identities -> metric-learning training ->
gallery enroll -> open-set query -> Rank-1/EER. No external dataset needed. This is the
"plumbing works" gate before plugging in PetFace + the real backbone.

Run:  python scripts/sanity_check.py [--head arcface|magface] [--epochs N]
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

# make `import petsy` work without installing
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "src"))

from petsy.capture import quality_gate          # noqa: E402
from petsy.config import Config, set_seed        # noqa: E402
from petsy.data import make_synthetic            # noqa: E402
from petsy.evaluate import run_pipeline          # noqa: E402


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--head", default="arcface", choices=["arcface", "magface", "softmax"])
    ap.add_argument("--epochs", type=int, default=14,
                    help="synthetic needs ~12-14 epochs to separate classes enough for the "
                         "strict open-set FMR threshold; rank-1 is already 1.0 by ~8")
    ap.add_argument("--backbone", default="small_cnn")
    args = ap.parse_args()

    set_seed(1337)
    cfg = Config(head=args.head, epochs=args.epochs, backbone=args.backbone)

    print("=" * 60)
    print("Biometric-Pet-Identification sanity check (synthetic data)")
    print("=" * 60)

    split = make_synthetic(seed=cfg.seed)

    # demonstrate the quality gate on a clean vs blurred sample
    clean = split.query_x[0]
    blurred = clean * 0 + clean.mean()        # flat image => no detail
    print(f"[quality] clean   -> {quality_gate(clean)}")
    print(f"[quality] flat    -> {quality_gate(blurred)}")

    results = run_pipeline(cfg, split, verbose=True)
    print("-" * 60)
    print("RESULTS:")
    print(json.dumps(results, indent=2))
    print("-" * 60)

    # sanity assertions: training should beat chance substantially
    ok = results["rank_1"] > 0.5 and results["open_set_accuracy"] > 0.5
    print("SANITY:", "PASS" if ok else "WEAK (tune epochs/lr)")
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
