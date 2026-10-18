"""Real-data loader: folder-per-identity -> the same SplitData contract as make_synthetic.

Expected layout (works for PetFace, nose-print sets, Flickr-dog, etc. after extraction):

    <root>/
        <individual_id_1>/  img001.jpg  img002.jpg ...
        <individual_id_2>/  ...

Splits are BY INDIVIDUAL (docs/DATA.md): a fraction of ids are held out as *unknown*
(appear only at query time -> open-set), the rest are *known* and split into
train / gallery / query. For very large sets (full PetFace), cap images/ids and later
switch to lazy DataLoaders; this eager path keeps `evaluate.run_pipeline` working unchanged.

Optional `metadata.csv` (id,breed,sex,color,...) can be loaded for soft-biometric
re-ranking — see load_metadata (re-ranking itself is a documented next step, not yet wired).
"""
from __future__ import annotations

import csv
import re
from pathlib import Path

import numpy as np

# chars invalid in Windows filenames — datasets created on Linux may use them in CSV refs,
# but on disk (after extraction) they get replaced with '_'. Match both forms.
_WIN_INVALID = re.compile(r'[*?<>:"|]')

from .data import SplitData

_IMG_EXT = {".jpg", ".jpeg", ".png", ".bmp", ".webp"}


def _load_image(path: Path, size: int, channels: int) -> np.ndarray:
    from PIL import Image
    mode = "RGB" if channels == 3 else "L"
    img = Image.open(path).convert(mode).resize((size, size))
    arr = np.asarray(img, dtype=np.float32) / 255.0
    if channels == 3:
        arr = np.transpose(arr, (2, 0, 1))           # HWC -> CHW
    else:
        arr = arr[None, ...]
    return arr


def scan_imagefolder(root: str | Path, min_images: int = 4) -> dict[str, list[Path]]:
    """Map identity-folder-name -> list of image paths (only ids with >= min_images)."""
    root = Path(root)
    out: dict[str, list[Path]] = {}
    for d in sorted(p for p in root.iterdir() if p.is_dir()):
        imgs = sorted(p for p in d.iterdir() if p.suffix.lower() in _IMG_EXT)
        if len(imgs) >= min_images:
            out[d.name] = imgs
    return out


def build_split(
    root: str | Path,
    *,
    img_size: int = 64,
    channels: int = 3,
    unknown_frac: float = 0.2,
    per_id_gallery: int = 2,
    per_id_query: int = 2,
    max_ids: int | None = None,
    max_per_id: int = 20,
    min_images: int = 4,
    seed: int = 1337,
) -> SplitData:
    """Build a SplitData from a folder-per-identity dataset (eager load)."""
    rng = np.random.default_rng(seed)
    folders = scan_imagefolder(root, min_images=min_images)
    if not folders:
        raise FileNotFoundError(f"no identities with >= {min_images} images under {root}")

    ids = list(folders)
    rng.shuffle(ids)
    if max_ids:
        ids = ids[:max_ids]
    id_to_label = {name: i for i, name in enumerate(ids)}

    n_unknown = max(1, int(len(ids) * unknown_frac))
    unknown_names = set(ids[:n_unknown])
    known_names = set(ids[n_unknown:])

    def load(name: str, n: int | None):
        paths = folders[name][:max_per_id]
        if n is not None:
            paths = paths[:n]
        return [_load_image(p, img_size, channels) for p in paths]

    tr_x, tr_y, g_x, g_y, q_x, q_y = [], [], [], [], [], []

    for name in known_names:
        label = id_to_label[name]
        imgs = load(name, None)
        rng.shuffle(imgs)
        n_g = min(per_id_gallery, max(1, len(imgs) // 3))
        n_q = min(per_id_query, max(1, len(imgs) // 3))
        gallery, query, train = imgs[:n_g], imgs[n_g:n_g + n_q], imgs[n_g + n_q:]
        if not train:                              # ensure at least 1 train sample
            train = [gallery.pop()]
        g_x += gallery; g_y += [label] * len(gallery)
        q_x += query;   q_y += [label] * len(query)
        tr_x += train;  tr_y += [label] * len(train)

    for name in unknown_names:                     # unknown -> query only (open-set)
        label = id_to_label[name]
        imgs = load(name, per_id_query)
        q_x += imgs; q_y += [label] * len(imgs)

    to = lambda a: np.asarray(a, dtype=np.float32)
    return SplitData(
        train_x=to(tr_x), train_y=np.asarray(tr_y, np.int64),
        gallery_x=to(g_x), gallery_y=np.asarray(g_y, np.int64),
        query_x=to(q_x), query_y=np.asarray(q_y, np.int64),
        known_ids={id_to_label[n] for n in known_names},
        unknown_ids={id_to_label[n] for n in unknown_names},
    )


def organize_by_label_csv(
    images_dir: str | Path,
    csv_path: str | Path,
    out_dir: str | Path,
    *,
    image_col: str = "image",
    id_col: str = "id",
    copy: bool = True,
) -> int:
    """Convert a flat image dir + labels CSV into folder-per-identity layout.

    Kaggle/challenge sets are usually flat (all images in one dir) with a CSV mapping
    image -> individual id. This builds <out_dir>/<id>/<image> so `build_split` can read it.
    Returns the number of images organized.
    """
    import shutil

    images_dir, out_dir = Path(images_dir), Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    # index available images by filename for robust lookup
    by_name = {p.name: p for p in images_dir.rglob("*") if p.suffix.lower() in _IMG_EXT}

    n = 0
    with open(csv_path, newline="", encoding="utf-8") as f:
        for row in csv.DictReader(f):
            img_ref, ident = row.get(image_col), row.get(id_col)
            if not img_ref or not ident:
                continue
            ref = Path(img_ref).name
            src = by_name.get(ref) or by_name.get(_WIN_INVALID.sub("_", ref))
            if src is None:
                src = images_dir / img_ref
            if not src.exists():
                continue
            dst_dir = out_dir / str(ident)
            dst_dir.mkdir(parents=True, exist_ok=True)
            dst = dst_dir / src.name
            if copy:
                shutil.copy2(src, dst)
            else:
                try:
                    dst.symlink_to(src.resolve())
                except OSError:
                    shutil.copy2(src, dst)   # symlink may need admin on Windows
            n += 1
    return n


def load_metadata(csv_path: str | Path) -> dict[str, dict]:
    """Optional soft-biometrics: id -> {breed,sex,color,...}. For future re-ranking."""
    out: dict[str, dict] = {}
    with open(csv_path, newline="", encoding="utf-8") as f:
        for row in csv.DictReader(f):
            key = row.get("id") or row.get("individual")
            if key:
                out[key] = row
    return out
