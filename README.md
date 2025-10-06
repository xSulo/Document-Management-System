# Document-Management-System (DMS)

Asynchronous document processing with **ASP.NET Core**, **PostgreSQL**, **RabbitMQ**, a **.NET Worker**, and a **React/NGINX** UI.

## Stack

* **API:** .NET 9, EF Core, AutoMapper
* **Database:** PostgreSQL 16
* **Queue:** RabbitMQ `3.13-management`
* **Worker:** .NET 9 BackgroundService (OCR stub)
* **UI:** React (Vite) served via NGINX

## Quick Start

```bash
docker compose up -d --build
```

* API → [http://localhost:8080](http://localhost:8080)
* UI → [http://localhost:8081](http://localhost:8081)
* RabbitMQ UI → [http://localhost:15672](http://localhost:15672)  (user **dev**, pass **dev**)
* Postgres → localhost:5432 (DB **dmsdb**, user **dms**, pass **dmsPW**)

---

## Sprint 1 – Base & REST

* EF Core + PostgreSQL connected
* `Document` entity + migration
* REST endpoints `/api/documents` (CRUD)
* Validation & AutoMapper in place

**Quick check**

```powershell
# List documents
curl.exe -s http://localhost:8080/api/documents
```

---

## Sprint 2 – UI & Webserver

* React UI (Vite) built and served via NGINX
* NGINX reverse-proxy routes `/api/*` to the API
* Upload form posts to `/api/documents`

Open: **[http://localhost:8081](http://localhost:8081)**

---

## Sprint 3 – Queues & Worker (RabbitMQ)

* **Exchange:** `dms.exchange` (topic)
* **Queue:** `dms.ocr`
* **Routing key:** `ocr.new`
* API publishes an `OcrJobMessage` after a successful upload
* .NET Worker consumes the message and logs it (OCR to be added later)

### End-to-End Test

**PowerShell (recommended):**

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/documents" `
  -ContentType 'application/json' `
  -Body '{"title":"Demo","filePath":"/files/demo.pdf"}'
```

**curl on Windows:**

```powershell
curl.exe -s -X POST "http://localhost:8080/api/documents" ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Demo\",\"filePath\":\"/files/demo.pdf\"}"
```

**Expected logs**

```bash
docker compose logs -f api ocrworker
```

* **api:** `Published message to ocr.new ...`
* **ocrworker:** `OCR worker received: DocumentId=..., Title=..., Path=...`

---

## Project Structure

```
dms/                 # API (ASP.NET Core)
dms.Ocr.Worker/      # Worker (BackgroundService)
ui/                  # React app
webserver/           # NGINX (Dockerfile + default.conf)
docker-compose.yml   # db + api + ui + rabbit + worker
```

## Configuration (from `docker-compose.yml`)

```yaml
RabbitMq__HostName: rabbit
RabbitMq__Port: 5672
RabbitMq__UserName: dev
RabbitMq__Password: dev
RabbitMq__Exchange: dms.exchange
RabbitMq__Queue: dms.ocr
RabbitMq__RoutingKey: ocr.new
```

## Troubleshooting

* **PowerShell quoting:** use `Invoke-RestMethod` or `curl.exe` (not the PS alias `curl`).
* **Worker Dockerfile:** lives in `dms.Ocr.Worker/` and builds with that folder as context.
* **RabbitMQ.Client version:** pinned to **6.8.x** in **API** and **Worker** to match the `IModel` API.
* **Worker `appsettings.json`:** must be valid JSON; environment variables from Compose override JSON values.
