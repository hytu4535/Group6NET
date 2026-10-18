"""Convert Oxford-IIIT Pet head-bbox XMLs -> YOLOv8 detection dataset.

Oxford gives one PASCAL-VOC head box per image with name cat|dog. We emit a YOLO dataset
(classes: 0=cat, 1=dog) to bootstrap Biometric-Pet-Identification's stage-[1] detector. The head box is a proxy
for the face crop; once you have nose-annotated data, add a 'nose' class the same way.

Run:
  python scripts/prepare_oxford_yolo.py                 # all 3,686 annotated images
  python scripts/prepare_oxford_yolo.py --limit 40      # quick smoke
  python scripts/prepare_oxford_yolo.py --copy          # copy images instead of symlink
"""
from __future__ import annotations

import argparse
import random
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "data" / "oxford_pet"
OUT = ROOT / "data" / "oxford_yolo"
CLASSES = {"cat": 0, "dog": 1}


def _link(src: Path, dst: Path, copy: bool):
    dst.parent.mkdir(parents=True, exist_ok=True)
    if dst.exists():
        return
    if copy:
        shutil.copy2(src, dst); return
    try:
        dst.symlink_to(src.resolve())
    except OSError:
        shutil.copy2(src, dst)          # symlink needs admin on Windows -> copy


def convert(limit: int | None, copy: bool, val_frac: float = 0.1, seed: int = 1337):
    xmls = sorted((SRC / "annotations" / "xmls").glob("*.xml"))
    if limit:
        xmls = xmls[:limit]
    random.Random(seed).shuffle(xmls)
    n_val = int(len(xmls) * val_frac)

    counts = {"train": 0, "val": 0, "skipped": 0}
    for i, xp in enumerate(xmls):
        split = "val" if i < n_val else "train"
        try:
            root = ET.parse(xp).getroot()
            fn = root.findtext("filename")
            W = int(root.findtext("size/width")); H = int(root.findtext("size/height"))
            obj = root.find("object")
            name = obj.findtext("name")
            if name not in CLASSES or W <= 0 or H <= 0:
                counts["skipped"] += 1; continue
            b = obj.find("bndbox")
            x1, y1 = float(b.findtext("xmin")), float(b.findtext("ymin"))
            x2, y2 = float(b.findtext("xmax")), float(b.findtext("ymax"))
            cx, cy = (x1 + x2) / 2 / W, (y1 + y2) / 2 / H
            bw, bh = (x2 - x1) / W, (y2 - y1) / H
        except Exception:
            counts["skipped"] += 1; continue

        img = SRC / "images" / fn
        if not img.exists():
            counts["skipped"] += 1; continue

        _link(img, OUT / "images" / split / fn, copy)
        lbl = OUT / "labels" / split / (Path(fn).stem + ".txt")
        lbl.parent.mkdir(parents=True, exist_ok=True)
        lbl.write_text(f"{CLASSES[name]} {cx:.6f} {cy:.6f} {bw:.6f} {bh:.6f}\n")
        counts[split] += 1

    (OUT / "pethead.yaml").write_text(
        f"path: {OUT.as_posix()}\ntrain: images/train\nval: images/val\n"
        f"names:\n  0: cat\n  1: dog\n"
    )
    return counts


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--limit", type=int, default=None)
    ap.add_argument("--copy", action="store_true")
    args = ap.parse_args()
    if not (SRC / "annotations" / "xmls").exists():
        raise SystemExit("Oxford data missing — run: python scripts/download_data.py --oxford-pet")
    c = convert(args.limit, args.copy)
    print(f"[ok] YOLO dataset -> {OUT}")
    print(f"     train={c['train']}  val={c['val']}  skipped={c['skipped']}")
    print(f"     config: {OUT / 'pethead.yaml'}")


if __name__ == "__main__":
    main()
