"""Convert a flat image dir + labels CSV into folder-per-identity layout for Biometric-Pet-Identification.

Most Kaggle/challenge nose-print sets ship as one flat folder of images plus a CSV that
maps each image to a dog id. Biometric-Pet-Identification's loader wants <root>/<id>/*.jpg. This bridges them.

Example:
  python scripts/organize_data.py --images data/dog-noses-image-data/images \
      --csv data/dog-noses-image-data/train.csv --image-col imageName --id-col dogId \
      --out data/dognose_byid
Then:
  build_split("data/dognose_byid", img_size=224)
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "src"))

from petsy.data_real import organize_by_label_csv   # noqa: E402


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--images", required=True)
    ap.add_argument("--csv", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--image-col", default="image")
    ap.add_argument("--id-col", default="id")
    ap.add_argument("--symlink", action="store_true", help="symlink instead of copy")
    args = ap.parse_args()

    n = organize_by_label_csv(args.images, args.csv, args.out,
                              image_col=args.image_col, id_col=args.id_col,
                              copy=not args.symlink)
    print(f"[ok] organized {n} images into folder-per-identity at {args.out}")


if __name__ == "__main__":
    main()
