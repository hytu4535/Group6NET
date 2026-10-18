"""Biometric-Pet-Identification Streamlit demo UI.

Run:  ./.venv/Scripts/python.exe -m streamlit run src/petsy/app.py

Enroll synthetic pets (done at build) and 'find' a pet: try a fresh photo of an enrolled
pet (-> match with owner) or an unknown pet (-> no confident match). Upload a real image
to see the quality gate + open-set safety in action.
"""
from __future__ import annotations

import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))  # src/ on path

import streamlit as st                              # noqa: E402

from petsy.serve_state import DemoState             # noqa: E402

st.set_page_config(page_title="Biometric-Pet-Identification 🐾", page_icon="🐾", layout="wide")


@st.cache_resource
def get_state() -> DemoState:
    return DemoState.build(verbose=False)


def to_image(arr: np.ndarray) -> np.ndarray:
    """(C,H,W) float[0,1] -> (H,W,C) uint8 for st.image."""
    a = np.clip(np.asarray(arr), 0, 1)
    if a.ndim == 3:
        a = np.transpose(a, (1, 2, 0))
    return (a * 255).astype(np.uint8)


def show_result(res: dict):
    if res.get("verdict") == "rejected":
        st.error(f"❌ Image rejected by quality gate: **{res['reason']}**")
        return
    v = res["verdict"]
    if v == "match":
        o = res["owner"]
        st.success(f"✅ **Match** — {o['name']} (owner: {o['owner']})  ·  "
                   f"similarity {res['best_similarity']}, P(same)={res['probability']}")
    elif v == "possible":
        st.warning(f"⚠️ **Possible match** — borderline (sim {res['best_similarity']}). "
                   "Ask a human to confirm.")
    else:
        st.info(f"🔍 **No confident match** (sim {res['best_similarity']}). "
                "This pet is likely not enrolled — open-set safety working.")
    st.caption("Top candidates")
    st.dataframe(res["candidates"], use_container_width=True, hide_index=True)


state = get_state()

st.title("Biometric-Pet-Identification 🐾 — biometric pet finder")
st.caption("Snap a photo of a dog → find its owner. Demo runs on synthetic pets; the "
           "pipeline (quality gate → embed → FAISS → open-set verdict) is the real one.")

with st.sidebar:
    st.header("Status")
    st.metric("Enrolled pets", len(state.profiles))
    st.write(f"**Device:** {state.device}")
    st.write(f"**Search backend:** {state.gallery.backend}")
    st.write(f"**Match threshold:** {state.decider.threshold:.3f}")
    st.divider()
    st.caption("Method: ArcFace→MagFace embeddings · FAISS gallery · FMR-gated open-set. "
               "See docs/research/METHOD_BENCHMARK.md")

tab_find, tab_pets = st.tabs(["🔎 Find a pet", "📇 Enrolled pets"])

with tab_find:
    c1, c2, c3 = st.columns(3)
    trigger = None
    if c1.button("Try an ENROLLED pet", use_container_width=True):
        trigger = ("known", None)
    if c2.button("Try an UNKNOWN pet", use_container_width=True):
        trigger = ("unknown", None)
    up = c3.file_uploader("…or upload an image", type=["png", "jpg", "jpeg"])
    if up is not None:
        from PIL import Image
        img = Image.open(up).convert("RGB").resize((state.cfg.img_size, state.cfg.img_size))
        arr = np.transpose(np.asarray(img, dtype=np.float32) / 255.0, (2, 0, 1))
        trigger = ("upload", arr)

    if trigger:
        kind, arr = trigger
        if kind in ("known", "unknown"):
            arr, _ = state.sample_query(known=(kind == "known"))
        left, right = st.columns([1, 2])
        with left:
            st.image(to_image(arr), caption="Query", width=200)
        with right:
            show_result(state.query_array(arr))

with tab_pets:
    st.write(f"{len(state.profiles)} pets enrolled.")
    cols = st.columns(6)
    # show one thumbnail per enrolled id from synthetic templates
    from petsy.data import make_synthetic
    s = make_synthetic(seed=state.cfg.seed)
    for n, (pid, prof) in enumerate(sorted(state.profiles.items())):
        idx = np.flatnonzero(s.gallery_y == pid)
        if idx.size:
            with cols[n % 6]:
                st.image(to_image(s.gallery_x[idx[0]]), caption=f"{prof['name']}\n({prof['owner']})",
                         width=90)
