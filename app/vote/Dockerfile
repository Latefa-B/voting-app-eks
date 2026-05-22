FROM python:3.11-slim

WORKDIR /app

# Copy requirements first so Docker caches this layer
# Rebuilds are faster if only app code changed (not requirements)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 80

CMD ["gunicorn", "app:app", "-b", "0.0.0.0:80", "--log-file", "-", "--access-logfile", "-", "--workers", "4", "--keep-alive", "0"]
