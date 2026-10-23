from pathlib import Path
import urllib.request

import cv2
import numpy as np
import onnxruntime as ort

from runtime_paths import MODEL_PATH

MODEL_URL = (
    "https://huggingface.co/open-noodle/pet-recognition-base/"
    "resolve/main/recognition/model.onnx"
)

def download_model():
    MODEL_PATH.parent.mkdir(parents=True, exist_ok=True)

    if MODEL_PATH.exists():
        print(f"[OK] Model already exists: {MODEL_PATH}")
        return

    print("[INFO] Downloading pet-recognition-base...")
    urllib.request.urlretrieve(MODEL_URL, MODEL_PATH)
    print(f"[OK] Model downloaded: {MODEL_PATH}")


def load_model():
    session = ort.InferenceSession(
        str(MODEL_PATH),
        providers=["CPUExecutionProvider"],
    )

    print("\n[MODEL]")
    for inp in session.get_inputs():
        print("Input :", inp.name, inp.shape, inp.type)

    for out in session.get_outputs():
        print("Output:", out.name, out.shape, out.type)

    return session


def preprocess(image_path: Path):
    image = cv2.imread(str(image_path))

    if image is None:
        raise FileNotFoundError(f"Cannot read image: {image_path}")

    # OpenCV: BGR -> RGB
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Model input: 224 x 224
    image = cv2.resize(image, (224, 224))

    # uint8 -> float32 [0, 1]
    image = image.astype(np.float32) / 255.0

    # ImageNet normalization
    mean = np.array(
        [0.485, 0.456, 0.406],
        dtype=np.float32,
    )

    std = np.array(
        [0.229, 0.224, 0.225],
        dtype=np.float32,
    )

    image = (image - mean) / std

    # HWC -> CHW
    image = np.transpose(image, (2, 0, 1))

    # Add batch dimension
    image = np.expand_dims(image, axis=0)

    return image.astype(np.float32)


def main():
    download_model()

    session = load_model()

    # Lấy tên input/output thật từ model
    input_name = session.get_inputs()[0].name
    output_name = session.get_outputs()[0].name

    # Tạm dùng ảnh test trong thư mục:
    # test_images/pet.jpg
    image_path = ROOT / "test_images" / "pet.jpg"

    if not image_path.exists():
        print(
            "\n[ERROR] Chưa có ảnh test."
        )
        print(
            f"Đặt một ảnh thú cưng tại:\n{image_path}"
        )
        return

    image = preprocess(image_path)

    print("\n[INFERENCE]")
    print("Input shape:", image.shape)

    embedding = session.run(
        [output_name],
        {input_name: image},
    )[0]

    embedding = np.asarray(embedding)

    print("Embedding shape:", embedding.shape)
    print("Embedding dtype:", embedding.dtype)

    # L2 normalize
    norm = np.linalg.norm(embedding, axis=1, keepdims=True)

    if np.any(norm == 0):
        raise ValueError("Embedding norm is zero.")

    embedding = embedding / norm

    print("Normalized shape:", embedding.shape)
    print(
        "Embedding norm:",
        np.linalg.norm(embedding, axis=1),
    )

    print("\nFirst 10 values:")
    print(embedding[0][:10])

    print("\n[SUCCESS] pet-recognition-base chạy OK.")


if __name__ == "__main__":
    main()