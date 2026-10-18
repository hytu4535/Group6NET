"""Train Biometric-Pet-Identification's stage-[1] detector (YOLOv8) on the Oxford pet-head dataset.

Bootstraps the detector that replaces `petsy/detect.py`'s stub: detect cat/dog + a head
(face proxy) box. Add a 'nose' class later from nose-annotated data for the full recipe.

Prereqs:
  pip install ultralytics          # pulls torch; use the CUDA build for real speed
  python scripts/prepare_oxford_yolo.py

Run:
  python scripts/train_detector.py --epochs 50 --imgsz 640
"""
from __future__ import annotations

import argparse
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_DATA = ROOT / "data" / "oxford_yolo" / "pethead.yaml"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--model", default="yolov8n.pt", help="n=nano (6GB-friendly)")
    ap.add_argument("--epochs", type=int, default=50)
    ap.add_argument("--imgsz", type=int, default=640)
    ap.add_argument("--batch", type=int, default=16)
    ap.add_argument("--device", default=None, help="e.g. 0 for GPU, cpu otherwise")
    ap.add_argument("--data", default=str(DEFAULT_DATA), help="path to a YOLO data.yaml")
    args = ap.parse_args()

    data = Path(args.data)
    if not data.exists():
        raise SystemExit(f"data.yaml not found: {data}")
    try:
        from ultralytics import YOLO
    except ImportError:
        raise SystemExit("ultralytics not installed — run: pip install ultralytics")

    model = YOLO(args.model)
    model.train(data=str(data), epochs=args.epochs, imgsz=args.imgsz,
                batch=args.batch, device=args.device, project=str(ROOT / "runs"),
                name="pethead-detector", seed=1337, deterministic=True)
    print("[ok] detector trained. Best weights under runs/pethead-detector/weights/best.pt")
    print("     Wire into petsy/detect.py to replace the stub.")


if __name__ == "__main__":
    main()
