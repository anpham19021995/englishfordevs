# Features

## Current MVP

- Email/password registration and login with JWT auth.
- AI text feedback for developer English practice.
- Practice modes:
  - AI chat practice
  - Mock interview practice
  - Vietnamese or mixed-language IT explanation conversion
- AI providers:
  - Ollama Cloud or local Ollama
  - OpenAI API
  - Local fallback feedback when provider calls fail
- Structured feedback:
  - Direct reply
  - Corrected version
  - Natural professional version
  - Vocabulary with short meaning/example
  - Confidence coaching
  - Follow-up question
- Saved practice history per account.
- Clear all saved history for the current account.
- Scrollable, filterable, collapsible history list for longer practice sessions.
- Developer vocabulary panel:
  - Static top vocabulary for software engineering communication
  - Personal vocabulary derived from AI feedback history
  - Search and category filters
- Progress tracking:
  - Total practices
  - Mode counts
  - Current streak
- PostgreSQL persistence when configured.
- In-memory fallback for local development.
- Health diagnostics for AI provider and history storage mode.
- Frontend system status panel for backend URL, AI provider, environment, and history storage.
- Local dev consistency check for backend URL, PostgreSQL, and AI provider configuration.
- Clear warning when local fallback feedback is shown because the AI provider is unavailable.
- Hardened backend validation for auth and practice requests.

## Retention

- Login sessions expire after 12 hours.
- PostgreSQL history persists until the user clears it or the database is reset.
- In-memory history is lost when the backend restarts.
- History API returns 20 recent items by default and clamps requests to 50 items.

# Feature Ideas

## AI Daily Standup

User practices daily scrum updates.

## PR Review Simulator

Practice code review communication.

## Architecture Discussion

Explain system design in English.
