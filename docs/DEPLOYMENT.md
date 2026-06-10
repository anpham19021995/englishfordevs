# Deployment Guide

Recommended MVP deployment:

- Frontend: Azure Static Web Apps
- Backend: Azure Container Apps
- Database: Neon Free PostgreSQL
- AI provider: Ollama Cloud
- Domain:
  - `www.yourdomain.com` -> Azure Static Web Apps frontend
  - `api.yourdomain.com` -> Azure Container Apps backend, optional for Phase 1

Current production shape:

- Frontend: `https://www.sampham.it.com`
- Frontend fallback URL: `https://polite-meadow-0bda1e610.7.azurestaticapps.net`
- Backend: `https://englishfordevs-api.redpond-758e4a2a.southeastasia.azurecontainerapps.io`
- Database: Neon PostgreSQL
- Ollama model: `gemma3:4b`
- Render backend: kept only as a temporary backup

## 1. Create PostgreSQL on Neon

1. Create a Neon project.
2. Copy the pooled or direct PostgreSQL connection string.
3. Keep it for the backend environment variable `DATABASE_CONNECTION_STRING`.

The backend applies EF Core migrations automatically on startup when a database connection string is configured.

## 2. Deploy Backend on Azure Container Apps

Create an Azure Container Registry:

```bash
Registry name=englishfordevsacr
Region=Southeast Asia
Pricing plan=Basic
```

Build and push the backend image from Azure Cloud Shell:

```bash
git clone https://github.com/anpham19021995/englishfordevs.git
cd englishfordevs

az acr build \
  --registry englishfordevsacr \
  --image englishfordevs-api:latest \
  --file backend/Dockerfile \
  .
```

Create an Azure Container App:

```bash
Container app name=englishfordevs-api
Environment=englishfordevs-env
Region=Southeast Asia
Image=englishfordevsacr.azurecr.io/englishfordevs-api:latest
Workload profile=Consumption
CPU and memory=0.5 CPU cores, 1 Gi memory
Ingress=Enabled
Ingress traffic=Accepting traffic from anywhere
Target port=8080
```

Set these environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DATABASE_CONNECTION_STRING=your_neon_postgres_connection_string
AI_PROVIDER=ollama
OLLAMA_API_KEY=your_ollama_api_key
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gemma3:4b
JWT_SECRET=replace_with_a_long_random_secret
CORS_ALLOWED_ORIGINS=https://www.sampham.it.com,https://polite-meadow-0bda1e610.7.azurestaticapps.net,http://localhost:3000
```

Store these as secure values when the Azure UI offers it:

- `DATABASE_CONNECTION_STRING`
- `OLLAMA_API_KEY`
- `JWT_SECRET`

After deploy, verify:

```bash
GET https://englishfordevs-api.redpond-758e4a2a.southeastasia.azurecontainerapps.io/api/health
GET https://englishfordevs-api.redpond-758e4a2a.southeastasia.azurecontainerapps.io/api/health/ai
```

The AI health response should report:

- `environment`: `Production`
- `provider`: `ollama`
- `historyStorage`: `postgres`
- `databaseConfigured`: `true`
- `jwtSecretConfigured`: `true`
- `ollamaApiKeyConfigured`: `true`
- `ollamaModel`: `gemma3:4b`

## 3. Deploy Frontend on Azure Static Web Apps

Azure Static Web Apps deploys from GitHub Actions.

The workflow is:

```bash
.github/workflows/azure-static-web-apps-polite-meadow-0bda1e610.yml
```

Important settings:

```yaml
app_location: "./frontend"
api_location: ""
output_location: ".next"
NEXT_PUBLIC_API_BASE_URL: https://englishfordevs-api.redpond-758e4a2a.southeastasia.azurecontainerapps.io
```

Push to `main` to trigger a frontend deploy.

## 4. Add Frontend Custom Domain

In Azure Static Web Apps:

1. Open the frontend resource.
2. Go to Settings -> Custom domains.
3. Add `www.sampham.it.com`.
4. Choose `Custom domain on other DNS`.
5. Add the DNS record shown by Azure at Namecheap.

Namecheap DNS:

```bash
Type=CNAME
Host=www
Value=polite-meadow-0bda1e610.7.azurestaticapps.net
TTL=Automatic
```

DNS can take time to propagate. If one network still opens an old site, wait for DNS/browser cache to expire or test from another network.

## 5. Optional Backend Custom Domain

For Phase 1, the generated Azure Container Apps backend URL is acceptable.

Later, add `api.sampham.it.com`:

1. Open the Azure Container App.
2. Go to Custom domains.
3. Add `api.sampham.it.com`.
4. Add the DNS record shown by Azure at Namecheap.
5. Update `NEXT_PUBLIC_API_BASE_URL` in the Azure Static Web Apps workflow.
6. Add the frontend origin to `CORS_ALLOWED_ORIGINS` and create a new backend revision.

## 6. Final Smoke Test

- Open `https://www.sampham.it.com`.
- Register or log in.
- Submit one practice message.
- Confirm the history item source is `Ollama`, not `local fallback`.
- Refresh the page and confirm history still loads.
- Check `/api/health/ai` and confirm `historyStorage` is `postgres`.

## Render Backup

Render was the original backend host and can be kept temporarily as a backup.

1. In Render, create a new Blueprint from the GitHub repository.
2. Render will read `render.yaml`.
3. Set these environment variables:

```bash
DATABASE_CONNECTION_STRING=your_neon_postgres_connection_string
AI_PROVIDER=ollama
OLLAMA_API_KEY=your_ollama_api_key
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gemma3:4b
CORS_ALLOWED_ORIGINS=https://www.yourdomain.com,https://your-vercel-project.vercel.app
```

`JWT_SECRET` is generated by Render from `render.yaml`. If you prefer, replace it with your own long random secret.

The database value can be either Npgsql keyword format:

```bash
Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

or a PostgreSQL URL:

```bash
postgresql://user:password@host/database?sslmode=require
```

If creating a Web Service manually instead of using Blueprint, choose Docker and set:

```bash
Dockerfile Path=backend/Dockerfile
Docker Build Context Directory=.
```

After deploy, verify:

```bash
GET https://api.yourdomain.com/api/health
GET https://api.yourdomain.com/api/health/ai
```

The AI health response should report:

- `environment`: `Production`
- `provider`: `ollama`
- `historyStorage`: `postgres`
- `ollamaApiKeyConfigured`: `true`

## Legacy Vercel Frontend

Vercel can still host the frontend, but Azure Static Web Apps is now the primary frontend host.

1. Import the GitHub repository into Vercel.
2. Set Root Directory to `frontend`.
3. Use the default Next.js build settings. If configuring manually:

```bash
Install Command=npm install
Build Command=npm run build
Output Directory=.next
```

4. Set this environment variable:

```bash
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
```

5. Deploy.

## Notes

- Azure Container Apps Consumption can cold start after inactivity.
- Keep `.env`, API keys, database URLs, and JWT secrets out of Git.
- If the frontend shows a CORS error, update `CORS_ALLOWED_ORIGINS` on Azure Container Apps and create a new backend revision.
- If feedback source is `local fallback`, check the Ollama model. `gemma3:4b` is the current working model; `gpt-oss:20b` returned empty content during testing.
