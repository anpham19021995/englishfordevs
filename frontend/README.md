# English for Developers Frontend

Next.js MVP for AI-powered English practice for software engineers.

## Setup

```bash
npm install
copy .env.local.example .env.local
npm run dev
```

Point the frontend at the ASP.NET Core backend in `.env.local`:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5200
```

AI configuration is handled by the backend.

## Current MVP

- AI chat practice
- Mock interview practice
- Vietnamese to professional English conversion
- Login and register through the backend API
- Stored token validation through the backend profile endpoint
- In-session practice history
- Saved backend history when authenticated
- Progress stats for authenticated users
- Structured feedback: corrected version, natural version, vocabulary, confidence, and follow-up question

## Frontend Structure

- `app/page.tsx`: page-level state orchestration
- `components/`: auth, progress, mode selector, composer, and history UI
- `lib/api.ts`: backend API client
- `lib/practiceModes.ts`: practice mode metadata
