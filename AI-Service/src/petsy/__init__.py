"""Biometric-Pet-Identification — biometric pet re-identification (dog nose-print + face).

v1 implements the evidence-selected method (see docs/research/METHOD_BENCHMARK.md):
metric-learning embedding (ArcFace baseline -> MagFace) + FAISS-style gallery +
open-set calibrated matching. The detection stage (YOLOv8) is stubbed until M1 data.
"""

__version__ = "0.1.0"
