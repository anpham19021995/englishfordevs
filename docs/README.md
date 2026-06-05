# English for Developers

AI-powered English learning platform for software engineers.

## Features

- AI mock interview
- IT conversation practice
- Grammar correction
- Vietnamese -> Professional English conversion

## Tech Stack

- ASP.NET Core
- Next.js
- PostgreSQL
- OpenAI API

## Screenshots

## Roadmap

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

If you debug the backend with Visual Studio F5 and use the PostgreSQL connection in user secrets, start PostgreSQL first:

```bash
npm run dev:postgres
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

To connect the frontend to the backend, set this in `frontend/.env.local`:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5200
```

The backend exposes:

- `GET /api/health`
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

Frontend OpenAI variables live in `frontend/.env.local`.

Backend OpenAI variables can be set with .NET user secrets, environment variables, or `backend/appsettings.Development.json`:

```bash
OpenAI__ApiKey=your_api_key_here
OpenAI__Model=gpt-4o-mini
Jwt__Secret=replace_with_a_long_random_secret
```

The practice endpoint uses OpenAI structured outputs so feedback matches the app contract:

- `directReply`
- `correctedVersion`
- `naturalVersion`
- `vocabulary`
- `confidenceFeedback`
- `followUpQuestion`

If no OpenAI key is configured, the backend returns local fallback feedback and marks the attempt source as `local-fallback`.

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
OPENAI_API_KEY=your_api_key_here
OPENAI_MODEL=gpt-4o-mini
OLLAMA_API_KEY=your_ollama_api_key_here
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gpt-oss:20b
JWT_SECRET=replace_with_a_long_random_secret
```

To use Ollama Cloud instead of OpenAI, set `AI_PROVIDER=ollama` and provide `OLLAMA_API_KEY`.
For local Ollama, use `OLLAMA_BASE_URL=http://host.docker.internal:11434/api` in Docker, or `http://localhost:11434/api` when running the backend directly.

Profile, practice, history, and progress endpoints require a JWT from `/api/auth/login` or `/api/auth/register`.
