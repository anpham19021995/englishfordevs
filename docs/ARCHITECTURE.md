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
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/practice`
- `GET /api/practice/history`
- JWT bearer authentication
- `GET /api/me`
- `GET /api/me/progress`

Database:

- PostgreSQL
- Practice history persistence when `ConnectionStrings:DefaultConnection` is configured
- In-memory fallback for local development without a database
- EF Core migrations applied on backend startup

AI:

- OpenAI API or Ollama API
- Structured feedback contract shared by frontend and backend
- JSON Schema structured outputs for reliable practice feedback parsing
- Local fallback feedback when no AI provider key is configured or a provider call fails

Backend folders:

- `Features/Auth`: register and login endpoint handlers
- `Features/Practice`: practice feedback and history endpoint handlers
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
