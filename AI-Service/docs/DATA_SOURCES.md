# Biometric-Pet-Identification — Data Sources & Acquisition

How to get training data online, and what to capture beyond pictures. Pairs with
`DATA.md` (datasheet) and `../data/LICENSES.md` (license gate). Helper: `scripts/download_data.py`.

## 1. Where to get it (verified access paths)

| Dataset | Use | Access | License |
|---------|-----|--------|---------|
| **PetFace** (ECCV 2024) | **Primary** individual re-ID + open-set verification; 257k individuals, +sex/breed/color/pattern | Google form → [github.com/mapooon/PetFace](https://github.com/mapooon/PetFace) (manual approval) | research (gated) |
| **CVPR 2022 Pet Biometric Challenge** | dog **nose-print** re-ID | Kaggle `nexdatafrank/dog-noses-image-data`; code [muzishen/Pet-ReID-IMAG](https://github.com/muzishen/Pet-ReID-IMAG) | challenge/research |
| **Nexdata dog nose** | 1,073 dogs / 64,378 imgs, clean license | [nexdata.ai/datasets/computervision/1051](https://www.nexdata.ai/datasets/computervision/1051) | **commercial (paid)** |
| **Oxford-IIIT Pet** | detection (**head bbox**), breed, species, segmentation | direct http (`images.tar.gz`,`annotations.tar.gz`) | **CC BY-SA 4.0 (commercial OK)** |
| **Stanford Dogs / Tsinghua Dogs** | breed + bbox, detector pretrain | direct http | research |
| **Cat Individual Images** | phase-2 cats | Kaggle `timost1234/cat-individuals` | check page |

## 2. The mechanics
- **Direct http** (Oxford): `python scripts/download_data.py --oxford-pet`
- **Kaggle** (ready — just add a token):
  1. One-time: kaggle.com → Settings → API → *Create New Token* → save `kaggle.json` to
     `~/.kaggle/kaggle.json` (or set `KAGGLE_USERNAME`/`KAGGLE_KEY`).
  2. `python scripts/download_data.py --dog-noseprint`   (or `--cat-individuals`, or `--kaggle <slug>`)
  - Without a token it prints these steps and exits cleanly — no crash.
- **Flat dir + labels CSV → folder-per-identity** (most Kaggle sets need this):
  `python scripts/organize_data.py --images <dir> --csv <labels.csv> --image-col <c> --id-col <c> --out <dir>_byid`
- **HuggingFace**: `huggingface-cli download <repo> --repo-type dataset`.
- **PetFace (gated)**: submit the Google form, wait for the link, extract into folder-per-id.
- Browse `python scripts/download_data.py --list` for the full catalog.

After download, point the loader at it:
```python
from petsy.data_real import build_split
split = build_split("data/petface_dog", img_size=224, max_ids=300, max_per_id=20)
# then petsy.evaluate.run_pipeline(cfg, split)
```
Expected layout: `data/<set>/<individual_id>/*.jpg` (folder per individual).

## 3. What to collect BEYOND pictures (makes the model stronger)
Ranked by impact:
1. **More views per individual** — nose + face + body, multiple poses/angles. Biggest lever.
2. **Clean identity labels** — "these N photos = same animal." Split by individual.
3. **Detection annotations** — face + **nose** bounding boxes + landmarks (YOLOv8 + alignment).
4. **Soft biometrics / metadata** — breed, coat color, pattern, size, sex, age, scars, ear/tail
   shape → shrink the search + re-rank (a `metadata.csv` is supported by `data_real.load_metadata`).
5. **Large pool of UNENROLLED animals** — impostors to calibrate the open-set false-match-rate.
6. **Hard negatives** — different animals of the same breed/color → forces real discrimination.
7. **Quality + environment metadata** — lighting, device, **geo/last-seen location** (location alone
   massively narrows a lost-pet search), timestamp.
8. **Video / multi-frame** — frame selection + temporal aggregation (Nose-to-ID).
9. **Microchip ID** when present — free ground-truth identity link.

## 4. Thai-specific data to gather (the moat — see business docs)
- **Thai soi / mixed-breed** photos (public sets under-represent them).
- **Phone-quality, real-world captures** (the Google-DR failure mode).
- Collected via shelter/vet pilots with **consent** (owner contact = PDPA personal data).

## 5. Before training: the license gate
Fill `../data/LICENSES.md` for every dataset. A model inherits the **most restrictive**
license it trains on — research-only data ⇒ research-only model. For a commercial product,
plan a clean-licensed (Oxford CC, Nexdata commercial, or own-collected) retraining path.
