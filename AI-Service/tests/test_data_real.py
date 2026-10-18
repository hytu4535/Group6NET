import numpy as np
import pytest

pytest.importorskip("PIL")

from PIL import Image  # noqa: E402

from petsy.data_real import build_split, organize_by_label_csv, scan_imagefolder  # noqa: E402


def _make_fake_dataset(root, n_ids=10, per_id=8, size=32):
    rng = np.random.default_rng(0)
    for i in range(n_ids):
        d = root / f"id_{i:03d}"
        d.mkdir(parents=True)
        base = rng.integers(0, 255, (8, 8, 3), dtype=np.uint8)
        for j in range(per_id):
            arr = np.repeat(np.repeat(base, size // 8, 0), size // 8, 1)
            arr = np.clip(arr.astype(int) + rng.integers(-20, 20, arr.shape), 0, 255).astype("uint8")
            Image.fromarray(arr).save(d / f"{j}.png")


def test_scan_and_build(tmp_path):
    _make_fake_dataset(tmp_path, n_ids=10, per_id=8)
    folders = scan_imagefolder(tmp_path, min_images=4)
    assert len(folders) == 10

    split = build_split(tmp_path, img_size=32, unknown_frac=0.2,
                        per_id_gallery=2, per_id_query=2, seed=1)
    # shapes
    assert split.train_x.shape[1:] == (3, 32, 32)
    assert len(split.train_y) > 0 and len(split.gallery_y) > 0 and len(split.query_y) > 0
    # ~20% unknown ids
    assert len(split.unknown_ids) == 2 and len(split.known_ids) == 8


def test_split_by_individual_no_leak(tmp_path):
    _make_fake_dataset(tmp_path, n_ids=10, per_id=9)
    split = build_split(tmp_path, img_size=32, seed=2)
    # unknown ids must NOT appear in train or gallery (open-set integrity)
    assert not (set(split.train_y) & split.unknown_ids)
    assert not (set(split.gallery_y) & split.unknown_ids)


def test_organize_by_label_csv(tmp_path):
    # simulate Kaggle flat dir + labels CSV
    flat = tmp_path / "flat"; flat.mkdir()
    rng = np.random.default_rng(0)
    rows = []
    for k in range(12):
        ident = f"dog{k % 3}"               # 3 dogs, 4 imgs each
        name = f"img_{k}.png"
        Image.fromarray(rng.integers(0, 255, (16, 16, 3), dtype="uint8")).save(flat / name)
        rows.append((name, ident))
    csv_path = tmp_path / "labels.csv"
    with open(csv_path, "w", encoding="utf-8") as f:
        f.write("image,id\n")
        for name, ident in rows:
            f.write(f"{name},{ident}\n")

    out = tmp_path / "byid"
    n = organize_by_label_csv(flat, csv_path, out, image_col="image", id_col="id")
    assert n == 12
    folders = scan_imagefolder(out, min_images=4)
    assert set(folders) == {"dog0", "dog1", "dog2"}
