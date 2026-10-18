import numpy as np
import pytest

pytest.importorskip("torch")
pytest.importorskip("fastapi")

from fastapi.testclient import TestClient   # noqa: E402

import petsy.api as api                      # noqa: E402
from petsy.config import Config             # noqa: E402
from petsy.serve_state import DemoState     # noqa: E402


@pytest.fixture(scope="module")
def client():
    # small, deterministic state for fast tests
    api._STATE = DemoState.build(Config(head="arcface", epochs=8),
                                 cache_dir="outputs/test_demo", force=True, verbose=False)
    return TestClient(api.app)


def test_health(client):
    r = client.get("/health")
    assert r.status_code == 200
    body = r.json()
    assert body["status"] == "ok"
    assert body["enrolled"] > 0


def test_pets_list(client):
    r = client.get("/pets")
    pets = r.json()["pets"]
    assert len(pets) > 0
    assert {"id", "name", "owner"} <= set(pets[0])


def test_search_known_returns_valid_verdict(client):
    r = client.post("/search", params={"demo": "known"})
    body = r.json()
    assert body["verdict"] in ("match", "possible", "no confident match")
    assert "candidates" in body and len(body["candidates"]) > 0


def test_search_unknown_not_falsely_matched(client):
    # an unknown pet should NOT be confidently matched to an owner (open-set safety)
    rejects = 0
    for _ in range(5):
        body = client.post("/search", params={"demo": "unknown"}).json()
        if body["verdict"] != "match":
            rejects += 1
    assert rejects >= 3   # majority correctly not asserted as a match


def test_enroll_new_pet(client):
    img = (np.random.default_rng(0).random((3, 64, 64)) * 0.5 + 0.25).astype(np.float32)
    # encode to PNG bytes
    from io import BytesIO

    from PIL import Image
    pil = Image.fromarray((np.transpose(img, (1, 2, 0)) * 255).astype("uint8"))
    buf = BytesIO(); pil.save(buf, format="PNG"); buf.seek(0)
    r = client.post("/enroll", data={"name": "TestPet", "owner": "Tester"},
                    files={"file": ("p.png", buf, "image/png")})
    body = r.json()
    assert body["enrolled"] is True
    assert body["name"] == "TestPet"
