"""Demo state shared by the API and the Streamlit UI.

Builds (and caches) a trained backbone + an enrolled gallery of named synthetic pets, plus
the open-set decider. Exposes enroll/query over the real pipeline:
    quality gate -> (detect stub) -> embed -> FAISS search -> calibrated open-set verdict.

This makes a coherent, runnable demo WITHOUT real pet photos: enroll synthetic pets with
owner names, then "find" a fresh photo of an enrolled pet (-> match) or an unknown pet
(-> no confident match). Real photos can be uploaded too (they'll read as 'no confident
match' against the synthetic gallery, which honestly demonstrates open-set safety).
"""
from __future__ import annotations

import json
from pathlib import Path

import numpy as np

from .calibrate import OpenSetDecider, platt_fit, platt_proba
from .capture import quality_gate
from .config import Config, set_seed
from .data import make_synthetic
from .gallery import Gallery
from .metrics import verification_scores
from .train import extract_embeddings, train

_PET_NAMES = ["Lucky", "Khao", "Mali", "Tofu", "Mochi", "Nong", "Bua", "Somtam",
              "Coco", "Ginger", "Pepper", "Mali2", "Tukta", "Maew", "Tong", "Dao"]
_OWNERS = ["Somchai", "Ploy", "Nattaya", "Anan", "Kwan", "Tee", "Mint", "Beam",
           "Fah", "Aran", "Nok", "Pim", "Got", "Ying", "Boss", "Mook"]


class DemoState:
    def __init__(self, cfg, backbone, gallery, decider, platt_ab, profiles, device):
        self.cfg = cfg
        self.backbone = backbone
        self.gallery = gallery
        self.decider = decider
        self.platt_ab = platt_ab
        self.profiles = profiles            # id -> {"name","owner"}
        self.device = device

    # ---------- pipeline ops ----------
    def _embed(self, img: np.ndarray) -> np.ndarray:
        x = np.asarray(img, dtype=np.float32)[None, ...]     # (1,C,H,W)
        return extract_embeddings(self.backbone, x, self.device)

    def query_array(self, img: np.ndarray, k: int = 5) -> dict:
        q = quality_gate(img)
        if not q.ok:
            return {"verdict": "rejected", "reason": q.reason,
                    "quality": {"ok": False, "reason": q.reason}}
        emb = self._embed(img)
        ids, sims = self.gallery.search(emb, k=k)
        ids, sims = ids[0], sims[0]
        best_sim = float(sims[0])
        verdict = self.decider.verdict(best_sim)
        prob = float(platt_proba([best_sim], self.platt_ab)[0])
        cand = [{"id": int(i), "owner": self.profiles[int(i)]["owner"],
                 "name": self.profiles[int(i)]["name"], "similarity": round(float(s), 4)}
                for i, s in zip(ids, sims)]
        owner = cand[0] if verdict == "match" else None
        return {"verdict": verdict, "probability": round(prob, 4),
                "best_similarity": round(best_sim, 4), "owner": owner,
                "candidates": cand, "quality": {"ok": True, "reason": "ok"}}

    def enroll_array(self, img: np.ndarray, name: str, owner: str) -> dict:
        q = quality_gate(img)
        if not q.ok:
            return {"enrolled": False, "reason": q.reason}
        emb = self._embed(img)
        new_id = (max(self.profiles) + 1) if self.profiles else 0
        self.profiles[new_id] = {"name": name, "owner": owner}
        self.gallery.enroll(emb, [new_id], [{"name": name, "owner": owner}])
        return {"enrolled": True, "id": new_id, "name": name, "owner": owner}

    # ---------- demo helpers ----------
    def sample_query(self, known: bool = True):
        """Return a FRESH photo of an enrolled (known) pet or an unknown pet.

        Uses cfg.seed so templates match the enrolled gallery; query_x holds fresh noisy
        draws of those same identities (plus unknown ids at the tail).
        """
        s = make_synthetic(seed=self.cfg.seed)
        mask = s.is_known(s.query_y)
        pool = np.flatnonzero(mask if known else ~mask)
        i = int(np.random.choice(pool))
        return s.query_x[i], int(s.query_y[i])

    # ---------- persistence ----------
    @classmethod
    def build(cls, cfg: Config | None = None, cache_dir: str = "outputs/demo",
              force: bool = False, verbose: bool = True) -> "DemoState":
        cfg = cfg or Config(head="magface", epochs=10)
        set_seed(cfg.seed)
        device = cfg.resolved_device()
        cache = Path(cache_dir)

        split = make_synthetic(seed=cfg.seed)
        from .models import get_backbone
        backbone = get_backbone(cfg.backbone, cfg.channels, cfg.embed_dim).to(device)

        weights = cache / "backbone.pt"
        if weights.exists() and not force:
            import torch
            backbone.load_state_dict(torch.load(weights, map_location=device))
            if verbose:
                print(f"[DemoState] loaded cached backbone from {weights}")
        else:
            if verbose:
                print("[DemoState] training backbone on synthetic data...")
            backbone, _ = train(cfg, split.train_x, split.train_y, verbose=verbose)
            cache.mkdir(parents=True, exist_ok=True)
            import torch
            torch.save(backbone.state_dict(), weights)

        # enroll one representative per known id, with names/owners
        g_emb = extract_embeddings(backbone, split.gallery_x, device)
        gallery = Gallery(dim=g_emb.shape[1])
        profiles: dict[int, dict] = {}
        seen = set()
        for emb, y in zip(g_emb, split.gallery_y):
            y = int(y)
            if y in seen:
                # still add extra samples (improves matching), reuse profile
                gallery.enroll(emb[None, :], [y], [profiles[y]])
                continue
            seen.add(y)
            name = _PET_NAMES[y % len(_PET_NAMES)] + (f"-{y}" if y >= len(_PET_NAMES) else "")
            owner = _OWNERS[y % len(_OWNERS)]
            profiles[y] = {"name": name, "owner": owner}
            gallery.enroll(emb[None, :], [y], [profiles[y]])

        # fit open-set decider + platt on the synthetic query split
        q_emb = extract_embeddings(backbone, split.query_x, device)
        top1_sim, top1_same = verification_scores(q_emb, split.query_y, g_emb, split.gallery_y)
        genuine = split.is_known(split.query_y) & top1_same
        decider = OpenSetDecider.fit(top1_sim, genuine, cfg.target_fmr, cfg.abstain_band)
        platt_ab = platt_fit(top1_sim, genuine)

        if verbose:
            print(f"[DemoState] ready: {len(profiles)} pets enrolled, "
                  f"backend={gallery.backend}, threshold={decider.threshold:.3f}")
        return cls(cfg, backbone, gallery, decider, platt_ab, profiles, device)
