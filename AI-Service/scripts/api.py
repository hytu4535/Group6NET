import json
import os

import cv2
import faiss
import numpy as np
import onnxruntime as ort

from fastapi import FastAPI, File, UploadFile, HTTPException
from runtime_paths import INDEX_PATH, MAPPING_PATH, MODEL_PATH


# ============================================================
# CONFIG
# ============================================================

MATCH_THRESHOLD = 0.75
MAYBE_THRESHOLD = 0.60

TOP_K = 30


# ============================================================
# LOAD MODEL
# ============================================================

print("[INFO] Loading ONNX model...")

session = ort.InferenceSession(
    str(MODEL_PATH),
    providers=["CPUExecutionProvider"]
)

input_name = session.get_inputs()[0].name

print(f"[OK] Model loaded: {MODEL_PATH}")


# ============================================================
# LOAD FAISS
# ============================================================

print("[INFO] Loading FAISS index...")

index = faiss.read_index(str(INDEX_PATH))

print(f"[OK] FAISS loaded")
print(f"[INFO] Vectors : {index.ntotal}")
print(f"[INFO] Dimension: {index.d}")


# ============================================================
# LOAD MAPPING
# ============================================================

with open(MAPPING_PATH, "r", encoding="utf-8") as f:
    mapping = json.load(f)

print(f"[OK] Mapping loaded: {len(mapping)} items")


# ============================================================
# FASTAPI
# ============================================================

app = FastAPI(
    title="Pet Recognition AI",
    description="AI service for individual pet recognition",
    version="1.0.0"
)


# ============================================================
# IMAGE PREPROCESSING
# ============================================================

def preprocess_image(image_bytes: bytes) -> np.ndarray:
    """
    Convert uploaded image into model input:
    RGB -> resize 224x224 -> ImageNet normalization
    """

    image_array = np.frombuffer(image_bytes, dtype=np.uint8)

    image = cv2.imdecode(
        image_array,
        cv2.IMREAD_COLOR
    )

    if image is None:
        raise ValueError("Cannot read image")

    # BGR -> RGB
    image = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2RGB
    )

    # Resize
    image = cv2.resize(
        image,
        (224, 224)
    )

    # float32
    image = image.astype(np.float32) / 255.0

    # ImageNet normalization
    mean = np.array(
        [0.485, 0.456, 0.406],
        dtype=np.float32
    )

    std = np.array(
        [0.229, 0.224, 0.225],
        dtype=np.float32
    )

    image = (image - mean) / std

    # HWC -> CHW
    image = np.transpose(
        image,
        (2, 0, 1)
    )

    # Add batch dimension
    image = np.expand_dims(
        image,
        axis=0
    )

    return image.astype(np.float32)


# ============================================================
# EMBEDDING
# ============================================================

def extract_embedding(image_bytes: bytes) -> np.ndarray:

    image = preprocess_image(image_bytes)

    output = session.run(
        None,
        {
            input_name: image
        }
    )[0]

    embedding = output[0].astype(np.float32)

    # L2 normalize
    norm = np.linalg.norm(embedding)

    if norm == 0:
        raise ValueError("Embedding norm is zero")

    embedding = embedding / norm

    return embedding


# ============================================================
# SEARCH FAISS
# ============================================================

def search_gallery(
    embedding: np.ndarray,
    top_k: int = TOP_K
):
    query = np.expand_dims(
        embedding,
        axis=0
    ).astype(np.float32)

    similarities, indices = index.search(
        query,
        min(top_k, index.ntotal)
    )

    pet_results = {}

    for index_id, similarity in zip(
        indices[0],
        similarities[0]
    ):
        if index_id < 0:
            continue

        item = mapping[index_id]
        pet_id = item["pet_id"]
        similarity = float(similarity)

        # Mỗi PetID chỉ giữ similarity cao nhất
        if (
            pet_id not in pet_results
            or similarity > pet_results[pet_id]["similarity"]
        ):
            pet_results[pet_id] = {
                "pet_id": pet_id,
                "similarity": similarity,
                "image": item["image"]
            }

    # Sắp xếp PetID theo similarity giảm dần
    results = sorted(
        pet_results.values(),
        key=lambda x: x["similarity"],
        reverse=True
    )

    # Chỉ lấy TOP-N PetID khác nhau
    return results[:5]

# ============================================================
# DETERMINE STATUS
# ============================================================

def determine_status(results):

    if not results:
        return "NOT_RECOGNIZED"

    best_similarity = results[0]["similarity"]

    if best_similarity >= MATCH_THRESHOLD:
        return "MATCH"

    if best_similarity >= MAYBE_THRESHOLD:
        return "MAYBE"

    return "NOT_RECOGNIZED"


# ============================================================
# HEALTH
# ============================================================

@app.get("/health")
def health():
    if index.ntotal == 0 or len(mapping) != index.ntotal:
        raise HTTPException(
            status_code=503,
            detail="Model/vector index is not ready or its mapping is inconsistent.",
        )

    return {
        "status": "ok",
        "ready": True,
        "model": "pet-recognition-base",
        "embedding_dimension": index.d,
        "gallery_vectors": index.ntotal,
    }


# ============================================================
# RECOGNIZE
# ============================================================

@app.post("/api/ai/recognize")
async def recognize(
    file: UploadFile = File(...)
):

    # Check image
    if not file.content_type:
        raise HTTPException(
            status_code=400,
            detail="Missing content type"
        )

    if not file.content_type.startswith("image/"):
        raise HTTPException(
            status_code=400,
            detail="File must be an image"
        )

    try:

        # Read uploaded image
        image_bytes = await file.read()

        if not image_bytes:
            raise HTTPException(
                status_code=400,
                detail="Empty image"
            )

        # Extract embedding
        embedding = extract_embedding(
            image_bytes
        )

        # Search FAISS
        results = search_gallery(
            embedding
        )

        # Determine status
        status = determine_status(
            results
        )

        # Best candidate
        best = results[0] if results else None

        return {
            "status": status,

            "pet_id": (
                best["pet_id"]
                if best
                else None
            ),

            "similarity": (
                best["similarity"]
                if best
                else None
            ),

            "candidates": [
                {
                    "pet_id": item["pet_id"],
                    "similarity": item["similarity"]
                }
                for item in results
            ]
        }

    except HTTPException:
        raise

    except Exception as e:

        raise HTTPException(
            status_code=500,
            detail=str(e)
        )
# ============================================================
# RUN SERVER
# ============================================================

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        app,
        host=os.getenv("PET_AI_HOST", "0.0.0.0"),
        port=int(os.getenv("PET_AI_PORT", "8000")),
    )