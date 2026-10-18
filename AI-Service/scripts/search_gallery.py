import json
import sys

import faiss
import numpy as np

from pet_embedder import PetEmbedder
from runtime_paths import INDEX_PATH, MAPPING_PATH

MATCH_THRESHOLD = 0.75
MAYBE_THRESHOLD = 0.60

def search(image_path: str, top_k: int = 5):
    embedder = PetEmbedder()

    index = faiss.read_index(
        str(INDEX_PATH)
    )

    with open(
        MAPPING_PATH,
        "r",
        encoding="utf-8",
    ) as file:
        mapping = json.load(file)

    query = embedder.embed(image_path)

    query = np.asarray(
        [query],
        dtype=np.float32,
    )

    similarities, indices = index.search(
        query,
        top_k,
    )

    print("\n==============================")
    print("RECOGNITION RESULTS")
    print("==============================")

    # Gom kết quả theo PetID
    pet_results = {}

    for index_id, similarity in zip(indices[0], similarities[0]):
        if index_id < 0:
            continue

        item = mapping[index_id]
        pet_id = item["pet_id"]

        # Giữ similarity cao nhất của mỗi PetID
        if pet_id not in pet_results:
            pet_results[pet_id] = {
                "similarity": float(similarity),
                "image": item["image"]
            }
        elif similarity > pet_results[pet_id]["similarity"]:
            pet_results[pet_id] = {
                "similarity": float(similarity),
                "image": item["image"]
            }

    # Sắp xếp PetID theo similarity giảm dần
    results = sorted(
        pet_results.items(),
        key=lambda x: x[1]["similarity"],
        reverse=True
    )
    if not results:
        status = "NOT_RECOGNIZED"
    else:
        best_pet_id, best_result = results[0]
        best_similarity = best_result["similarity"]

        if best_similarity >= MATCH_THRESHOLD:
            status = "MATCH"
        elif best_similarity >= MAYBE_THRESHOLD:
            status = "MAYBE"
        else:
            status = "NOT_RECOGNIZED"

    print("\n==============================")
    print("PET CANDIDATES")
    print("==============================")

    for rank, (pet_id, result) in enumerate(results, start=1):
        print(
            f"{rank}. PetID={pet_id} "
            f"similarity={result['similarity']:.4f} "
            f"image={result['image']}"
        )
    print("\n==============================")
    print("FINAL RESULT")
    print("==============================")
    
    if results:
        best_pet_id, best_result = results[0]
    
        print(f"Status     : {status}")
        print(f"PetID      : {best_pet_id}")
        print(f"Similarity : {best_result['similarity']:.4f}")
    else:
        print("Status : NOT_RECOGNIZED")

    


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage:")
        print("python scripts/search_gallery.py <image_path>")
        raise SystemExit(1)

    test_image = sys.argv[1]

    search(
        test_image,
        top_k=5,
    )