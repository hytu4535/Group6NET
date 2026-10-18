"""End-to-end evaluation: train -> enroll gallery -> query -> closed-set + open-set metrics."""
from __future__ import annotations

import numpy as np

from .calibrate import OpenSetDecider
from .config import Config
from .data import SplitData
from .gallery import Gallery
from .metrics import eer, rank_k_and_map, verification_scores
from .train import extract_embeddings, train


def run_pipeline(cfg: Config, split: SplitData, *, verbose=True) -> dict:
    device = cfg.resolved_device()
    if verbose:
        print(f"[Biometric-Pet-Identification] device={device} backbone={cfg.backbone} head={cfg.head}")

    backbone, history = train(cfg, split.train_x, split.train_y, verbose=verbose)

    g_emb = extract_embeddings(backbone, split.gallery_x, device)
    q_emb = extract_embeddings(backbone, split.query_x, device)

    # ---- gallery enroll
    gallery = Gallery(dim=g_emb.shape[1])
    gallery.enroll(g_emb, split.gallery_y,
                   meta=[{"owner": f"owner_{int(i)}"} for i in split.gallery_y])

    # ---- closed-set metrics on KNOWN queries only
    known_mask = split.is_known(split.query_y)
    closed = rank_k_and_map(q_emb[known_mask], split.query_y[known_mask],
                            g_emb, split.gallery_y, ks=cfg.rank_ks)

    # ---- open-set: top-1 sim per query + same/diff label across ALL queries
    top1_sim, top1_same = verification_scores(q_emb, split.query_y, g_emb, split.gallery_y)
    # a query is genuinely matchable only if its id is enrolled (known)
    genuine = known_mask & top1_same
    open_eer = eer(top1_sim, genuine)
    decider = OpenSetDecider.fit(top1_sim, genuine, cfg.target_fmr, cfg.abstain_band)
    verdicts = np.array(decider.verdicts(top1_sim))

    # open-set accuracy: known->match(correct id), unknown->no confident match
    matched = verdicts == "match"
    known_correct = (matched & known_mask & top1_same).sum()
    unknown_rejected = ((~matched) & (~known_mask)).sum()
    open_acc = (known_correct + unknown_rejected) / len(verdicts)

    results = {
        "device": device,
        "gallery_backend": gallery.backend,
        "n_train": int(len(split.train_y)),
        "n_gallery": int(len(split.gallery_y)),
        "n_query": int(len(split.query_y)),
        "final_train_loss": round(history[-1], 4),
        **{k: round(v, 4) for k, v in closed.items()},
        "open_set_eer": round(open_eer, 4),
        "open_set_threshold": round(decider.threshold, 4),
        "open_set_accuracy": round(float(open_acc), 4),
    }
    return results
