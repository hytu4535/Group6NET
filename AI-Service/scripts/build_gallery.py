import json

import faiss
import numpy as np

from pet_embedder import PetEmbedder
from runtime_paths import GALLERY_DIR, INDEX_PATH, MAPPING_PATH, VECTOR_STORE_DIR


def build_gallery():
    VECTOR_STORE_DIR.mkdir(parents=True, exist_ok=True)
    INDEX_PATH.parent.mkdir(parents=True, exist_ok=True)
    MAPPING_PATH.parent.mkdir(parents=True, exist_ok=True)

    embedder = PetEmbedder()

    vectors = []
    mapping = []

    # Mỗi thư mục con = một PetID
    #
    # gallery/
    # ├── PET001/
    # │   ├── 1.jpg
    # │   └── 2.jpg
    # │
    # └── PET002/
    #     └── 1.jpg

    if not GALLERY_DIR.exists():
        print(f"[ERROR] Gallery chưa tồn tại:")
        print(GALLERY_DIR)
        return

    pet_dirs = [
        path
        for path in GALLERY_DIR.iterdir()
        if path.is_dir()
    ]

    if not pet_dirs:
        print("[ERROR] Chưa có Pet nào trong gallery.")
        return

    for pet_dir in sorted(pet_dirs):
        pet_id = pet_dir.name

        image_files = []

        for extension in ("*.jpg", "*.jpeg", "*.png"):
            image_files.extend(
                pet_dir.glob(extension)
            )

        if not image_files:
            print(
                f"[WARNING] {pet_id} không có ảnh."
            )
            continue

        print(f"\nProcessing {pet_id}...")

        for image_path in sorted(image_files):
            try:
                embedding = embedder.embed(
                    str(image_path)
                )

                vectors.append(embedding)

                mapping.append(
                    {
                        "pet_id": pet_id,
                        "image": str(image_path.relative_to(GALLERY_DIR.parent)),
                    }
                )

                print(
                    f"  [OK] {image_path.name}"
                )

            except Exception as e:
                print(
                    f"  [ERROR] {image_path.name}: {e}"
                )

    if not vectors:
        print("[ERROR] Không tạo được embedding nào.")
        return

    matrix = np.asarray(
        vectors,
        dtype=np.float32,
    )

    # Embedding đã L2 normalize.
    # Inner Product = cosine similarity.
    dimension = matrix.shape[1]

    index = faiss.IndexFlatIP(dimension)

    index.add(matrix)

    faiss.write_index(
        index,
        str(INDEX_PATH),
    )

    with open(
        MAPPING_PATH,
        "w",
        encoding="utf-8",
    ) as file:
        json.dump(
            mapping,
            file,
            ensure_ascii=False,
            indent=2,
        )

    print("\n==============================")
    print("FAISS GALLERY CREATED")
    print("==============================")
    print(f"Vectors : {index.ntotal}")
    print(f"Dimension: {dimension}")
    print(f"Index   : {INDEX_PATH}")
    print(f"Mapping : {MAPPING_PATH}")


if __name__ == "__main__":
    build_gallery()