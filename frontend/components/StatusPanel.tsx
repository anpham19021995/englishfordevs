import type { BackendStatus } from "@/lib/api";
import { aiProviders, historyStorageTypes } from "@/lib/constants";
import { AlertTriangle, CheckCircle2, Server, WifiOff } from "lucide-react";

type StatusPanelProps = {
  apiBaseUrl: string;
  status: BackendStatus | null;
  error: string;
  isLoading: boolean;
  onRefresh: () => void;
};

export function StatusPanel({
  apiBaseUrl,
  status,
  error,
  isLoading,
  onRefresh,
}: StatusPanelProps) {
  const historyIsPersistent = status?.historyStorage === historyStorageTypes.postgres;
  const provider = status?.provider || "unknown";
  const providerKeyConfigured =
    provider === aiProviders.ollama
      ? status?.ollamaApiKeyConfigured
      : status?.openAiApiKeyConfigured;

  return (
    <div className="status-card">
      <div className="status-card-header">
        <div>
          <strong>Backend status</strong>
          <span>{apiBaseUrl || "API URL missing"}</span>
        </div>
        <button
          className="icon-button"
          type="button"
          aria-label="Refresh backend status"
          title="Refresh backend status"
          onClick={onRefresh}
          disabled={isLoading}
        >
          <Server size={15} aria-hidden="true" />
        </button>
      </div>

      {error ? (
        <StatusRow
          tone="danger"
          icon={WifiOff}
          label="Unavailable"
          value={error}
        />
      ) : null}

      {status ? (
        <>
          <StatusRow
            tone={providerKeyConfigured ? "ok" : "warning"}
            icon={providerKeyConfigured ? CheckCircle2 : AlertTriangle}
            label="AI provider"
            value={`${formatProvider(provider)}${
              providerKeyConfigured ? "" : " key missing"
            }`}
          />
          <StatusRow
            tone={historyIsPersistent ? "ok" : "warning"}
            icon={historyIsPersistent ? CheckCircle2 : AlertTriangle}
            label="History"
            value={
              historyIsPersistent
                ? "PostgreSQL"
                : "In-memory, resets on backend restart"
            }
          />
          <StatusRow
            tone="neutral"
            icon={Server}
            label="Environment"
            value={status.environment || "unknown"}
          />
        </>
      ) : !error ? (
        <StatusRow
          tone="neutral"
          icon={Server}
          label="Status"
          value={isLoading ? "Checking backend" : "Not checked"}
        />
      ) : null}
    </div>
  );
}

function StatusRow({
  tone,
  icon: Icon,
  label,
  value,
}: {
  tone: "ok" | "warning" | "danger" | "neutral";
  icon: typeof Server;
  label: string;
  value: string;
}) {
  return (
    <div className={`status-row ${tone}`}>
      <Icon size={16} aria-hidden="true" />
      <div>
        <span>{label}</span>
        <strong>{value}</strong>
      </div>
    </div>
  );
}

function formatProvider(provider: string) {
  if (provider === aiProviders.ollama) {
    return "Ollama";
  }

  if (provider === aiProviders.openAi) {
    return "OpenAI";
  }

  return provider || "Unknown";
}
