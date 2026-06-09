# Phase 1 Release Note

Phase 1 is the first usable MVP of English for Developers. The goal is to let a software engineer sign in, practice realistic workplace English, receive AI feedback, and review progress/history across sessions.

## Release Status

- Status: MVP complete
- Target user: software engineers practicing English for work
- Primary flow: sign in -> choose practice mode -> submit text -> receive feedback -> review history/progress

## Completed Checklist

- [x] Next.js frontend practice workspace
- [x] ASP.NET Core backend API
- [x] Email/password registration
- [x] JWT login/session handling
- [x] Protected practice, history, profile, and progress endpoints
- [x] AI chat practice mode
- [x] Mock interview practice mode
- [x] Vietnamese or mixed-language IT explanation converter mode
- [x] OpenAI provider support
- [x] Ollama Cloud/local provider support
- [x] Local fallback feedback when AI provider fails
- [x] Structured feedback contract:
  - [x] Direct reply
  - [x] Corrected version
  - [x] Natural professional version
  - [x] Vocabulary
  - [x] Confidence coaching
  - [x] Follow-up question
- [x] Feedback prompt quality improved for developer workplace English
- [x] Practice history saved per account
- [x] Clear current user's practice history
- [x] Scrollable, filterable, collapsible history UX
- [x] Developer vocabulary panel with static top phrases and history-derived personal phrases
- [x] User progress tracking
- [x] PostgreSQL persistence when configured
- [x] In-memory fallback for local development
- [x] EF Core migrations
- [x] Docker Compose for backend and PostgreSQL
- [x] Visual Studio/local debug setup documented
- [x] Backend AI/status health endpoint
- [x] Frontend provider/status panel
- [x] Better frontend error messages for auth, backend, AI fallback, and expired sessions
- [x] Backend validation hardening for auth and practice requests
- [x] Frontend validation limits aligned with backend limits
- [x] Shared constants for provider names, config keys, storage types, and validation limits
- [x] Backend integration/unit tests for core API flows
- [x] Frontend production build verified
- [x] Developer documentation updated

## Validation Rules

- Email must be valid and 256 characters or fewer.
- Password must be 8 to 128 characters.
- Practice message must be 3 to 4000 characters.
- Practice mode must be `chat`, `interview`, or `converter`.

## Manual Acceptance Checklist

- [ ] Start PostgreSQL with `npm run dev:postgres`.
- [ ] Start backend with Visual Studio F5 or `npm run dev:backend`.
- [ ] Start frontend with `npm run dev:frontend`.
- [ ] Confirm frontend points to the active backend URL.
- [ ] Confirm `/api/health/ai` reports the expected provider and `historyStorage`.
- [ ] Register a new account.
- [ ] Login with the same account.
- [ ] Submit feedback in all three practice modes.
- [ ] Confirm AI provider feedback appears when configured.
- [ ] Confirm local fallback warning appears if the provider is unavailable.
- [ ] Refresh the frontend and confirm history still loads.
- [ ] Restart backend with PostgreSQL configured and confirm history still loads.
- [ ] Clear history and confirm progress/history reset for the current user only.

## Verified Commands

```bash
dotnet test backend.tests/EnglishForDevs.Api.Tests.csproj -c Release
npm run build:frontend
```

## Known Limitations

- In-memory history is for local development only and is lost when the backend restarts.
- Frontend automated tests are not included in Phase 1.
- AI feedback quality depends on the configured provider/model.
- Streaming responses are not included in Phase 1.
- Voice input and pronunciation scoring are planned for Phase 2.

## Release Decision

Phase 1 is ready as a usable MVP. The app supports the core learning loop end to end and has enough diagnostics, validation, and persistence support for local demos and continued product iteration.
