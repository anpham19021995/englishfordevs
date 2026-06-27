# English for Developers API

ASP.NET Core Web API skeleton for the English for Developers platform.

## Run

```bash
dotnet run
```

From the project root:

```bash
npm run dev:backend
```

The local non-Docker API uses `http://localhost:5000`. Point `frontend/.env.local` at that URL when debugging with Visual Studio F5 or `dotnet run`.

When debugging the API with Visual Studio F5, start PostgreSQL first:

```bash
npm run dev:postgres
```

Check local config consistency:

```bash
npm run dev:check
```

With Docker from the project root:

```bash
copy .env.example .env
npm run docker:up
```

The containerized API is exposed at `http://localhost:5200`.
Point `frontend/.env.local` at `http://localhost:5200` only when using the Docker API.

Stop containers:

```bash
npm run docker:down
```

Reset containers and local PostgreSQL data:

```bash
npm run docker:down:volumes
```

## Endpoints

- `GET /api/health`
- `GET /api/health/ai`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/me`
- `GET /api/me/progress`
- `POST /api/practice`
- `POST /api/practice/transcribe`
- `POST /api/practice/tts`
- `GET /api/practice/history`
- `DELETE /api/practice/history`

Example practice request:

```json
{
  "mode": "chat",
  "message": "I fixed bug in API but explain not good."
}
```

Send practice and history requests with:

```http
Authorization: Bearer your_jwt_here
```

Supported modes:

- `chat`
- `interview`
- `converter`

## OpenAI Configuration

The API returns local fallback feedback when no key is configured.
When a key is configured, `/api/practice` calls OpenAI and requests structured JSON feedback that matches the frontend contract.
The API can also use Ollama by setting `AI:Provider` to `ollama`.
The root `.env` file is loaded for local development, so Visual Studio F5 can read the same AI and database settings even if the process starts as `Production`.

Use environment variables or .NET user secrets:

```bash
AI__Provider=openai
OpenAI__ApiKey=your_api_key_here
OpenAI__Model=gpt-4o-mini
Jwt__Secret=replace_with_a_long_random_secret
```

Ollama Cloud:

```bash
AI__Provider=ollama
Ollama__ApiKey=your_ollama_api_key_here
Ollama__BaseUrl=https://ollama.com/api
Ollama__Model=gemma3:4b
```

Local Ollama:

```bash
AI__Provider=ollama
Ollama__BaseUrl=http://localhost:11434/api
Ollama__Model=llama3.1:8b
```

Root `.env` equivalents:

```bash
AI_PROVIDER=ollama
OLLAMA_API_KEY=your_ollama_api_key_here
OLLAMA_BASE_URL=https://ollama.com/api
OLLAMA_MODEL=gemma3:4b
AZURE_SPEECH_KEY=your_azure_speech_key_here
AZURE_SPEECH_REGION=southeastasia
AZURE_SPEECH_VOICE=en-US-JennyNeural
JWT_SECRET=replace_with_a_long_random_secret
```

For local Visual Studio debugging:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Secret" "replace_with_a_long_random_secret"
dotnet user-secrets set "OpenAI:ApiKey" "your_api_key_here"
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini"
```

## Authentication

Register:

```json
{
  "email": "dev@example.com",
  "password": "password123"
}
```

The response includes a JWT. Practice attempts are saved with the authenticated user id. Profile, history, and progress only return data for that user.

## History Storage

The API uses in-memory practice history when no database is configured.

To persist history in PostgreSQL, set:

```bash
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

Or in root `.env`:

```bash
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

Check the active storage mode:

```http
GET /api/health/ai
```

The response includes `historyStorage`. Use `postgres` for persistent history. If it is `in-memory`, history disappears when the backend restarts.
The response also includes whether JWT and AI provider keys are configured, without exposing their values.

For local development, the API applies pending EF Core migrations automatically when the PostgreSQL connection is configured.

Docker Compose configures PostgreSQL automatically with:

```bash
Host=postgres;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

## Retention

- JWT login tokens expire after 12 hours.
- PostgreSQL history persists until the user clears it or the database is reset.
- In-memory history is process-local and lost on backend restart.
- History returns 20 recent items by default and supports up to 50 items per request.

## Migrations

Create a migration:

```bash
dotnet ef migrations add MigrationName --output-dir Data/Migrations
```

Apply migrations manually:

```bash
dotnet ef database update
```

## Tests

From the project root:

```bash
npm run test:backend
```

Backend tests use the API's in-memory auth/history fallback, so Docker and PostgreSQL are not required.
