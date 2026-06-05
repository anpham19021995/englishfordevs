# Architecture

Frontend:

- Next.js
- App Router
- Direct calls to the backend API through `NEXT_PUBLIC_API_BASE_URL`
- Client-side JWT storage for the current MVP auth flow

Backend:

- ASP.NET Core Web API
- Minimal API endpoints
- Lightweight vertical feature slices under `Features/`
- CQRS-style handler classes for commands and queries without MediatR
- `GET /api/health`
- `GET /api/health/ai`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/practice`
- `GET /api/practice/history`
- `DELETE /api/practice/history`
- JWT bearer authentication
- `GET /api/me`
- `GET /api/me/progress`
- Root `.env` loading for local AI/database configuration, including Visual Studio F5 scenarios
- `npm run dev:check` validates frontend backend URL, Postgres availability, AI provider config, and backend health

Database:

- PostgreSQL
- Practice history persistence when `ConnectionStrings:DefaultConnection` is configured
- In-memory fallback for local development without a database
- EF Core migrations applied on backend startup
- Health check reports `historyStorage` as `postgres` or `in-memory`
- Health check reports whether database and JWT configuration are present without exposing secret values

AI:

- OpenAI API or Ollama API
- Structured feedback contract shared by frontend and backend
- JSON Schema structured outputs for reliable practice feedback parsing
- Prompt rules optimize for exact learner message correction, professional developer wording, vocabulary with short explanations, and concrete confidence coaching
- Local fallback feedback when no AI provider key is configured or a provider call fails

Backend folders:

- `Features/Auth`: register and login endpoint handlers
- `Features/Practice`: practice feedback, history, and clear-history endpoint handlers
- `Features/Me`: authenticated user progress queries
- `Features/Health`: health endpoint mapping
- `Data`: EF Core context, entities, migrations
- `Services`: OpenAI, auth, and history storage infrastructure
- `Shared`: small cross-feature helpers

Hosting:

- Vercel + Render
- Docker Compose for local backend + PostgreSQL development

Realtime:

- SignalR
- Planned for streaming and realtime practice sessions
