# English for Developers

AI-powered English learning platform for software engineers.

## Features

- AI mock interview
- IT conversation practice
- Grammar correction
- Vietnamese -> Professional English conversion
- Saved practice history and progress tracking
- Clear saved practice history per account
- Filterable, collapsible history list for longer sessions
- Developer vocabulary with top phrases and history-derived personal terms

## Tech Stack

- ASP.NET Core
- Next.js
- PostgreSQL
- OpenAI API or Ollama API

## Screenshots

## Roadmap

- [Phase 1 release note](PHASE1_RELEASE.md)
- [Product roadmap](ROADMAP.md)
- [Deployment guide](DEPLOYMENT.md)

## Running locally

From the project root:

```bash
npm run install:frontend
npm run dev:frontend
```

Or from the frontend folder:

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:3000` by default.

Backend API:

```bash
npm run dev:backend
```

## Local Dev Modes

Use one backend mode at a time:

- Visual Studio F5 or `npm run dev:backend`: backend at `http://localhost:5000`
- Docker Compose API: backend at `http://localhost:5200`

For Visual Studio F5 or `dotnet run`, set `frontend/.env.local` to:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

For Docker Compose API, set it to:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5200
```

If you debug the backend with Visual Studio F5 and want persistent history, start PostgreSQL first:

```bash
npm run dev:postgres
```

Then make sure the backend reports PostgreSQL storage:

```bash
GET http://localhost:5000/api/health/ai
```

The response should include `"historyStorage": "postgres"`. If it says `"in-memory"`, history will be lost when the backend process restarts.
The frontend sidebar also shows backend URL, AI provider, environment, and history storage status.

Run the local dev consistency check:

```bash
npm run dev:check
```

Backend API with Docker and PostgreSQL:

```bash
copy .env.example .env
npm run docker:up
```

The Docker API is available at `http://localhost:5200`, and PostgreSQL is available at `localhost:5432`.

To stop Docker services:

```bash
npm run docker:down
```

To stop services and remove the local PostgreSQL volume:

```bash
npm run docker:down:volumes
```

To connect the frontend to the Docker backend, set this in `frontend/.env.local`:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5200
```

The backend exposes:

- `GET /api/health`
- `GET /api/health/ai`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/me`
- `GET /api/me/progress`
- `POST /api/practice`
- `GET /api/practice/history`
- `DELETE /api/practice/history`

Build everything from the root:

```bash
npm run build
```

Run backend tests:

```bash
npm run test:backend
```

## Database Migrations

The backend uses EF Core migrations. When a PostgreSQL connection string is configured, the API applies pending migrations on startup.

Create a new migration after changing entities:

```bash
dotnet ef migrations add MigrationName --project backend/EnglishForDevs.Api.csproj --startup-project backend/EnglishForDevs.Api.csproj --output-dir Data/Migrations
```

Apply migrations manually:

```bash
dotnet ef database update --project backend/EnglishForDevs.Api.csproj --startup-project backend/EnglishForDevs.Api.csproj
```

## Environment

Frontend variables live in `frontend/.env.local`.

Backend variables can be set with .NET user secrets, environment variables, or the root `.env` file:

```bash
AI_PROVIDER=ollama
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
OLLAMA_API_KEY=your_ollama_api_key_here
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gemma3:4b
JWT_SECRET=replace_with_a_long_random_secret
CORS_ALLOWED_ORIGINS=http://localhost:3000
```

The practice endpoint requests structured feedback matching the app contract:

- `directReply`
- `correctedVersion`
- `naturalVersion`
- `vocabulary` as `phrase - meaning/example`
- `confidenceFeedback`
- `followUpQuestion`

If no AI provider key is configured, or the provider call fails, the backend returns local fallback feedback and marks the attempt source as `local-fallback`.
The frontend shows a warning when fallback feedback is displayed.

Backend history storage uses in-memory storage by default. To persist practice history in PostgreSQL, set:

```bash
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

For Visual Studio/local backend debugging, prefer .NET user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres" --project backend/EnglishForDevs.Api.csproj
dotnet user-secrets set "Jwt:Secret" "replace_with_a_long_random_secret" --project backend/EnglishForDevs.Api.csproj
dotnet user-secrets set "OpenAI:ApiKey" "your_api_key_here" --project backend/EnglishForDevs.Api.csproj
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini" --project backend/EnglishForDevs.Api.csproj
```

When using Docker Compose, the API receives this connection string automatically:

```bash
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

Docker Compose also reads these values from the root `.env` file:

```bash
AI_PROVIDER=openai
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
OPENAI_API_KEY=your_api_key_here
OPENAI_MODEL=gpt-4o-mini
OLLAMA_API_KEY=your_ollama_api_key_here
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gemma3:4b
JWT_SECRET=replace_with_a_long_random_secret
```

To use Ollama Cloud instead of OpenAI, set `AI_PROVIDER=ollama` and provide `OLLAMA_API_KEY`.
For local Ollama, use `OLLAMA_BASE_URL=http://host.docker.internal:11434/api` in Docker, or `http://localhost:11434/api` when running the backend directly.

Profile, practice, history, and progress endpoints require a JWT from `/api/auth/login` or `/api/auth/register`.

## Validation Rules

- Email must be valid and 256 characters or fewer.
- Password must be 8 to 128 characters.
- Practice message must be 3 to 4000 characters.
- Practice mode must be `chat`, `interview`, or `converter`.

## Session and History Retention

- Login JWTs expire after 12 hours.
- PostgreSQL practice history is kept until the user clears it or the database is reset.
- In-memory fallback history is lost when the backend restarts.
- History fetch returns 20 items by default and supports up to 50 items per request.
