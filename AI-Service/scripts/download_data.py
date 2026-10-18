"""Biometric-Pet-Identification dataset download helper.

Downloads the license-clean PUBLIC sets directly; prints instructions for gated/Kaggle/
commercial ones. ALWAYS check + record licenses in data/LICENSES.md before training.

Usage:
  python scripts/download_data.py --list                 # show catalog, download nothing
  python scripts/download_data.py --oxford-pet           # direct http (~800MB, CC BY-SA 4.0)
  # gated/kaggle ones print instructions only.
"""
from __future__ import annotations

import argparse
import os
import sys
import tarfile
import urllib.request
from pathlib import Path

DATA = Path(__file__).resolve().parents[1] / "data"

KAGGLE_SLUGS = {
    "dog-noseprint": "nexdatafrank/dog-noses-image-data",
    "cat-individuals": "timost1234/cat-individuals",
}

CATALOG = {
    "oxford-pet": {
        "license": "CC BY-SA 4.0 (commercial OK)",
        "use": "detection (head bbox) + breed/species; alignment",
        "method": "direct-http",
        "urls": [
            "https://thor.robots.ox.ac.uk/datasets/pets/images.tar.gz",
            "https://thor.robots.ox.ac.uk/datasets/pets/annotations.tar.gz",
        ],
    },
    "petface": {
        "license": "research (gated)",
        "use": "PRIMARY individual re-ID + open-set verification (257k individuals)",
        "method": "form",
        "info": "Fill the Google form at https://github.com/mapooon/PetFace -> manual approval.",
    },
    "dog-noseprint": {
        "license": "challenge/research",
        "use": "dog nose-print re-ID (CVPR 2022 Pet Biometric Challenge)",
        "method": "kaggle",
        "info": "kaggle datasets download -d nexdatafrank/dog-noses-image-data ; "
                "code ref: github.com/muzishen/Pet-ReID-IMAG",
    },
    "nexdata-nose": {
        "license": "COMMERCIAL (paid)",
        "use": "1,073 dogs / 64,378 nose images (clean commercial license)",
        "method": "purchase",
        "info": "https://www.nexdata.ai/datasets/computervision/1051 (only if a commercial "
                "license is needed for a shipping product).",
    },
    "cat-individuals": {
        "license": "varies (check Kaggle page)",
        "use": "phase-2 cat individual ID",
        "method": "kaggle",
        "info": "kaggle datasets download -d timost1234/cat-individuals",
    },
}


def _download(url: str, dest: Path):
    dest.parent.mkdir(parents=True, exist_ok=True)
    print(f"  downloading {url}")

    def hook(b, bs, total):
        if total > 0:
            pct = min(100, b * bs * 100 // total)
            sys.stdout.write(f"\r    {pct}%"); sys.stdout.flush()

    urllib.request.urlretrieve(url, dest, hook)
    print("  done.")


def get_oxford_pet():
    out = DATA / "oxford_pet"
    for url in CATALOG["oxford-pet"]["urls"]:
        fn = out / url.split("/")[-1]
        _download(url, fn)
        print(f"  extracting {fn.name}")
        with tarfile.open(fn) as t:
            t.extractall(out)
    print(f"[ok] Oxford-IIIT Pet -> {out}")


def _kaggle_token_present() -> bool:
    if os.environ.get("KAGGLE_USERNAME") and os.environ.get("KAGGLE_KEY"):
        return True
    return (Path.home() / ".kaggle" / "kaggle.json").exists()


def _kaggle_setup_message():
    print(
        "\n[!] Kaggle API token not found. One-time setup:\n"
        "    1. Create a free account at https://www.kaggle.com\n"
        "    2. Account -> Settings -> API -> 'Create New Token' (downloads kaggle.json)\n"
        f"    3. Move it to:  {Path.home() / '.kaggle' / 'kaggle.json'}\n"
        "       (or set env vars KAGGLE_USERNAME and KAGGLE_KEY)\n"
        "    4. Re-run this command. Everything else is ready.\n"
    )


def kaggle_download(slug: str, name: str | None = None):
    """Download + unzip a Kaggle dataset (imports kaggle only after token check)."""
    if not _kaggle_token_present():
        _kaggle_setup_message()
        sys.exit(2)
    name = name or slug.split("/")[-1]
    out = DATA / name
    out.mkdir(parents=True, exist_ok=True)
    import kaggle  # safe now: token exists, so import-time auth succeeds
    print(f"  downloading {slug} -> {out}")
    kaggle.api.dataset_download_files(slug, path=str(out), unzip=True)
    print(f"[ok] {slug} -> {out}\n"
          f"     If images are flat + a labels CSV, convert to folder-per-identity:\n"
          f"     python scripts/organize_data.py --images {out} --csv <labels.csv> "
          f"--image-col <col> --id-col <col> --out {out}_byid")


def show_catalog():
    print("Biometric-Pet-Identification dataset catalog\n" + "=" * 60)
    for key, d in CATALOG.items():
        print(f"\n[{key}]  license: {d['license']}")
        print(f"  use   : {d['use']}")
        print(f"  method: {d['method']}")
        if d.get("info"):
            print(f"  info  : {d['info']}")
    print("\n>>> record every dataset's license in data/LICENSES.md before training. <<<")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--list", action="store_true")
    ap.add_argument("--oxford-pet", action="store_true")
    ap.add_argument("--dog-noseprint", action="store_true", help="Kaggle dog nose-print set")
    ap.add_argument("--cat-individuals", action="store_true", help="Kaggle cat individuals set")
    ap.add_argument("--kaggle", metavar="SLUG", help="download any Kaggle dataset by slug")
    args = ap.parse_args()

    if args.oxford_pet:
        get_oxford_pet(); return
    if args.dog_noseprint:
        kaggle_download(KAGGLE_SLUGS["dog-noseprint"]); return
    if args.cat_individuals:
        kaggle_download(KAGGLE_SLUGS["cat-individuals"]); return
    if args.kaggle:
        kaggle_download(args.kaggle); return

    show_catalog()
    if not args.list:
        print("\n(no download flag; try --oxford-pet, --dog-noseprint, --cat-individuals, "
              "--kaggle <slug>, or follow the instructions above)")


if __name__ == "__main__":
    main()
