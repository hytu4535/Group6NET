"""Multi-branch embedding fusion for dog and cat individual recognition.

Dog:
    face + nose + body

Cat:
    face + body

Each branch has its own backbone. Branch embeddings are concatenated
and L2-normalised to produce one identity descriptor.

The descriptor is used for FAISS nearest-neighbour search.
"""
from __future__ import annotations

import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import DataLoader, TensorDataset

from .config import Config, set_seed
from .models import build_head, get_backbone, l2_normalize


class MultiBranchEmbedder(nn.Module):
    def __init__(self, cfg: Config, species: str):
        super().__init__()
        species = species.lower()
        if species not in {"dog", "cat"}:
            raise ValueError(
                f"Unsupported species: {species}. "
                "Expected 'dog' or 'cat'. "
            )
        self.species = species

        # Face branch
        self.face = get_backbone(
            cfg.backbone,
            cfg.channels,
            cfg.embed_dim
        )

        # Body branch
        self.body = get_backbone(
            cfg.backbone,
            cfg.channels,
            cfg.embed_dim
        )

        # Dog has an additional nose branch
        if self.species == "dog":
            self.nose = get_backbone(
                cfg.backbone,
                cfg.channels,
                cfg.embed_dim
            )
        else:
            self.nose = None

    @property
    def num_branches(self) -> int:
        return 3 if self.species == "dog" else 2

    @property
    def output_dim(self) -> int:
        return self.num_branches * self._embed_dim

    @property
    def _embed_dim(self) -> int:
        if hasattr(self.face, "proj"):
            return self.face.proj.out_features

        if hasattr(self.face, "num_classes"):
            return self.face.num_classes

        raise RuntimeError(
            "Cannot determine embedding dimension from backbone"
        )

    def forward(
        self,
        face_x: torch.Tensor,
        body_x: torch.Tensor,
        nose_x: torch.Tensor | None = None,
    ) -> torch.Tensor:
        face_emb = self.face(face_x)
        body_emb = self.body(body_x)

        embeddings = [
            face_emb,
            body_emb
        ]

        if self.species == "dog":
            if nose_x is None:
                raise ValueError(
                    "Dog recognition requires nose_x."
                )
            nose_emb = self.nose(nose_x)
            embeddings.append(nose_emb)
        elif nose_x is not None:
            raise ValueError(
                "Cat recognition does not use nose_x"
            )

        return torch.cat(embeddings, dim = 1)


def train_fusion(
    cfg: Config,
    face_x: np.ndarray,
    body_x: np.ndarray,
    y: np.ndarray,
    *,
    nose_x: np.ndarray | None = None,
    species: str = "dog",
    verbose: bool = True,
):
    species = species.lower()

    if species not in {"dog", "cat"}:
        raise ValueError(
            f"Unsupported species: {species}. "
            "Expected 'dog' or 'cat'."
        )

    if species == "dog" and nose_x is None:
        raise ValueError(
            "nose_x is required when training the dog model."
        )

    if species == "cat" and nose_x is not None:
        raise ValueError(
            "nose_x should not be provided for the cat model."
        )

    set_seed(cfg.seed)

    device = cfg.resolved_device()
    use_amp = cfg.amp and device == "cuda"

    num_classes = int(y.max()) + 1

    model = MultiBranchEmbedder(
        cfg,
        species=species,
    ).to(device)

    head = build_head(
        cfg.head,
        model.output_dim,
        num_classes,
        cfg,
    ).to(device)

    opt = torch.optim.AdamW(
        list(model.parameters()) + list(head.parameters()),
        lr=cfg.lr,
        weight_decay=cfg.weight_decay,
    )

    scaler = torch.amp.GradScaler(
        "cuda",
        enabled=use_amp,
    )

    ce = nn.CrossEntropyLoss()

    # Dataset
    tensors = [
        torch.from_numpy(face_x),
        torch.from_numpy(body_x),
    ]

    if species == "dog":
        tensors.append(torch.from_numpy(nose_x))

    tensors.append(torch.from_numpy(y))

    ds = TensorDataset(*tensors)

    loader = DataLoader(
        ds,
        batch_size=cfg.batch_size,
        shuffle=True,
    )

    #Training
    model.train()
    head.train()

    history = []

    for epoch in range(cfg.epochs):

        total_loss = 0.0
        n = 0

        for batch in loader:

            if species == "dog":
                face_batch, body_batch, nose_batch, y_batch = batch

                face_batch = face_batch.to(device)
                body_batch = body_batch.to(device)
                nose_batch = nose_batch.to(device)
                y_batch = y_batch.to(device)

            else:
                face_batch, body_batch, y_batch = batch

                face_batch = face_batch.to(device)
                body_batch = body_batch.to(device)
                y_batch = y_batch.to(device)

                nose_batch = None

            opt.zero_grad(set_to_none=True)

            with torch.autocast(
                device_type="cuda",
                enabled=use_amp,
            ):

                emb = model(
                    face_x=face_batch,
                    body_x=body_batch,
                    nose_x=nose_batch,
                )

                logits, reg = head(
                    emb,
                    y_batch,
                )

                loss = ce(logits, y_batch) + reg

            scaler.scale(loss).backward()
            scaler.step(opt)
            scaler.update()

            total_loss += float(loss.detach()) * len(y_batch)
            n += len(y_batch)

        epoch_loss = total_loss / max(n, 1)
        history.append(epoch_loss)

        if verbose:
            print(
                f"  [{species} fusion] "
                f"epoch {epoch + 1}/{cfg.epochs} "
                f"loss={epoch_loss:.4f}"
            )

    return model, history


@torch.no_grad()
def extract_fused(
    model: MultiBranchEmbedder,
    face_x: np.ndarray,
    body_x: np.ndarray,
    device: str,
    *,
    nose_x: np.ndarray | None = None,
    batch: int = 256,
) -> np.ndarray:
    model.eval()

    if model.species == "dog" and nose_x is None:
        raise ValueError(
            "nose_x is required when extracting dog embeddings."
        )

    if model.species == "cat" and nose_x is not None:
        raise ValueError(
            "nose_x should not be provided when extracting cat embeddings."
        )

    face_t = torch.from_numpy(face_x)
    body_t = torch.from_numpy(body_x)

    if nose_x is not None:
        nose_t = torch.from_numpy(nose_x)
    else:
        nose_t = None

    out = []

    for i in range(0, len(face_t), batch):

        face_batch = face_t[i:i + batch].to(device)
        body_batch = body_t[i:i + batch].to(device)

        if nose_t is not None:
            nose_batch = nose_t[i:i + batch].to(device)
        else:
            nose_batch = None

        emb = model(
            face_x=face_batch,
            body_x=body_batch,
            nose_x=nose_batch,
        )

        emb = l2_normalize(emb)

        out.append(
            emb.cpu().numpy()
        )

    return np.concatenate(
        out,
        axis=0,
    ).astype(np.float32)
