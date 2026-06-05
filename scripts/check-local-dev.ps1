$ErrorActionPreference = "Stop"

function Read-DotEnv($path) {
    $values = @{}

    if (-not (Test-Path $path)) {
        return $values
    }

    Get-Content $path |
        Where-Object { $_ -match '^\s*[^#][^=]*=' } |
        ForEach-Object {
            $key, $value = $_ -split '=', 2
            $values[$key.Trim()] = $value.Trim().Trim('"', "'")
        }

    return $values
}

$root = Split-Path -Parent $PSScriptRoot
$rootEnv = Read-DotEnv (Join-Path $root ".env")
$frontendEnv = Read-DotEnv (Join-Path $root "frontend\.env.local")
$apiBaseUrl = $frontendEnv["NEXT_PUBLIC_API_BASE_URL"]

Write-Host "EnglishForDevs local dev check"
Write-Host ""

if ([string]::IsNullOrWhiteSpace($apiBaseUrl)) {
    Write-Host "[warn] frontend/.env.local is missing NEXT_PUBLIC_API_BASE_URL"
} else {
    Write-Host "[ok] frontend API URL: $apiBaseUrl"
}

if ([string]::IsNullOrWhiteSpace($rootEnv["DATABASE_CONNECTION_STRING"]) -and
    [string]::IsNullOrWhiteSpace($rootEnv["ConnectionStrings__DefaultConnection"])) {
    Write-Host "[warn] .env has no database connection; backend will use in-memory storage unless user-secrets provide one"
} else {
    Write-Host "[ok] .env database connection configured"
}

$provider = $rootEnv["AI_PROVIDER"]
if ([string]::IsNullOrWhiteSpace($provider)) {
    Write-Host "[warn] .env AI_PROVIDER missing"
} else {
    Write-Host "[ok] AI provider: $provider"
}

if ($provider -eq "ollama" -and [string]::IsNullOrWhiteSpace($rootEnv["OLLAMA_API_KEY"])) {
    Write-Host "[warn] OLLAMA_API_KEY missing"
}

if ($provider -eq "openai" -and [string]::IsNullOrWhiteSpace($rootEnv["OPENAI_API_KEY"])) {
    Write-Host "[warn] OPENAI_API_KEY missing"
}

$postgres = Get-NetTCPConnection -LocalPort 5432 -State Listen -ErrorAction SilentlyContinue
if ($postgres) {
    Write-Host "[ok] PostgreSQL port 5432 is listening"
} else {
    Write-Host "[warn] PostgreSQL port 5432 is not listening; run npm run dev:postgres"
}

if (-not [string]::IsNullOrWhiteSpace($apiBaseUrl)) {
    try {
        $status = Invoke-RestMethod -Uri "$apiBaseUrl/api/health/ai" -TimeoutSec 5
        Write-Host "[ok] backend reachable"
        Write-Host "     environment: $($status.environment)"
        Write-Host "     historyStorage: $($status.historyStorage)"
        Write-Host "     provider: $($status.provider)"

        if ($status.historyStorage -ne "postgres") {
            Write-Host "[warn] history is not persistent; configure DATABASE_CONNECTION_STRING and restart backend"
        }
    } catch {
        Write-Host "[warn] backend is not reachable at $apiBaseUrl"
    }
}
