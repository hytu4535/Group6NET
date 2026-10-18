FROM python:3.11-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PET_AI_ROOT=/app \
    PET_MODEL_PATH=/app/models/pet-recognition-base.onnx \
    PET_FAISS_INDEX_PATH=/app/vector_store/pets.faiss \
    PET_MAPPING_PATH=/app/vector_store/mapping.json \
    PET_AI_HOST=0.0.0.0 \
    PET_AI_PORT=8000

WORKDIR /app

RUN apt-get update \
    && apt-get install --no-install-recommends -y \
        libglib2.0-0 \
        libgomp1 \
        libstdc++6 \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt ./
RUN python -m pip install --no-cache-dir --upgrade pip \
    && python -m pip install --no-cache-dir -r requirements.txt

COPY scripts/api.py scripts/pet_embedder.py scripts/runtime_paths.py ./scripts/

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=5s --start-period=120s --retries=5 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://127.0.0.1:8000/health', timeout=4)" || exit 1

CMD ["python", "scripts/api.py"]
