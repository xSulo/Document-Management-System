# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# --- Run (mit .NET Runtime) ---
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Tesseract + Ghostscript (PDF -> PNG) installieren
RUN apt-get update && \
    apt-get install -y --no-install-recommends tesseract-ocr ghostscript ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# optional: deutsche Sprache für OCR
# RUN apt-get update && apt-get install -y tesseract-ocr-deu && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OcrWorker.dll"]
