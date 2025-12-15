# src/OcrWorker/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# 1. Installiere alle System-Abhängigkeiten
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libtesseract-dev \
    libleptonica-dev \
    tesseract-ocr-eng \
    ghostscript \
    libgdiplus \
    libc6-dev \
    libjpeg62-turbo \
    libpng16-16 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# 2. FIX: libdl (Wichtig für .NET auf Debian 12!)
# Ohne das stürzt der Loader oft ab, bevor er Tesseract lädt.
RUN ln -sf /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so

# 3. FIX: Tesseract Wrapper Struktur
# Der C# Wrapper (Version 5.2) sucht auf Linux im Unterordner "x64" nach spezifischen Dateinamen.
RUN mkdir -p /app/x64

# Wir KOPIEREN (cp) die System-Dateien hart auf die Namen, die C# erwartet.
RUN cp /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN cp /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so

# Zur Sicherheit auch im Root-Verzeichnis (Doppelt hält besser)
RUN cp /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/libleptonica-1.82.0.so
RUN cp /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/libtesseract50.so

# 4. Umgebungsvariablen
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/5/tessdata
# Wir fügen den x64 Ordner explizit zum Suchpfad hinzu
ENV LD_LIBRARY_PATH=/app:/app/x64:/usr/lib/x86_64-linux-gnu

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OcrWorker.dll"]