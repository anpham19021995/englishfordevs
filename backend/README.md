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

When debugging the API with Visual Studio F5, start PostgreSQL first:

```bash
npm run dev:postgres
```

With Docker from the project root:

```bash
copy .env.example .env
npm run docker:up
```

The containerized API is exposed at `http://localhost:5200`.

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
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/me`
- `GET /api/me/progress`
- `POST /api/practice`
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

Use environment variables or .NET user secrets:

```bash
OpenAI__ApiKey=your_api_key_here
OpenAI__Model=gpt-4o-mini
Jwt__Secret=replace_with_a_long_random_secret
```

Ollama Cloud:

```bash
AI__Provider=ollama
Ollama__ApiKey=your_ollama_api_key_here
Ollama__BaseUrl=https://ollama.com/api
Ollama__Model=gpt-oss:20b
```

Local Ollama:

```bash
AI__Provider=ollama
Ollama__BaseUrl=http://localhost:11434/api
Ollama__Model=llama3.1:8b
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

For local development, the API applies pending EF Core migrations automatically when the PostgreSQL connection is configured.

Docker Compose configures PostgreSQL automatically with:

```bash
Host=postgres;Port=5432;Database=english_for_devs;Username=postgres;Password=postgres
```

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
