"""
Stage [1] — pet detection and crop extraction.

Output:
    Dog:
        face_crop
        nose_crop
        body_crop

    Cat:
        face_crop
        body_crop
        nose_crop = None

"""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np


@dataclass
class Detection:
    species: str            # "dog" | "cat" | "unknown"
    face_crop: np.ndarray
    body_crop: np.ndarray
    nose_crop: np.ndarray | None


def detect(img: np.ndarray) -> Detection:
    """Temporary detector interface.

    This is still a STUB.

    For now the whole image is used as the body/face crop so
    the downstream multi-branch pipeline can be tested.

    TODO:
        Replace this function with real YOLO inference that
        detects:
            Dog -> face + nose + body
            Cat -> face + body
    """

    image = np.asarray(img, dtype=np.float32)

    return Detection(
        species="dog",
        face_crop=image,
        body_crop=image,
        nose_crop=image,
    )
