import cv2
import numpy as np
import onnxruntime as ort

from runtime_paths import MODEL_PATH


class PetEmbedder:
    def __init__(self):
        self.session = ort.InferenceSession(
            str(MODEL_PATH),
            providers=["CPUExecutionProvider"],
        )

        self.input_name = self.session.get_inputs()[0].name
        self.output_name = self.session.get_outputs()[0].name

    def preprocess(self, image_path: str) -> np.ndarray:
        image = cv2.imread(image_path)

        if image is None:
            raise FileNotFoundError(
                f"Cannot read image: {image_path}"
            )

        # BGR -> RGB
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        # Model input: 224 x 224
        image = cv2.resize(image, (224, 224))

        # Convert to float
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

    def embed(self, image_path: str) -> np.ndarray:
        image = self.preprocess(image_path)

        embedding = self.session.run(
            [self.output_name],
            {
                self.input_name: image
            },
        )[0]

        embedding = np.asarray(
            embedding,
            dtype=np.float32,
        )

        # L2 normalize
        norm = np.linalg.norm(
            embedding,
            axis=1,
            keepdims=True,
        )

        embedding = embedding / np.maximum(norm, 1e-12)

        return embedding[0]