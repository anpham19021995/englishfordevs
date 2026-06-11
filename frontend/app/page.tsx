"use client";

import { AuthPanel } from "@/components/AuthPanel";
import { HistoryList } from "@/components/HistoryList";
import { ModeSelector } from "@/components/ModeSelector";
import { PracticeComposer } from "@/components/PracticeComposer";
import { ProgressPanel } from "@/components/ProgressPanel";
import { StatusPanel } from "@/components/StatusPanel";
import { VocabularyPanel } from "@/components/VocabularyPanel";
import {
  AuthResponse,
  ApiRequestError,
  BackendStatus,
  clearHistory,
  getApiBaseUrl,
  getBackendStatus,
  getHistory,
  getProfile,
  getProgress,
  hasApiBaseUrl,
  HistoryItem,
  login,
  PracticeMode,
  register,
  submitPractice,
  UserProgress,
} from "@/lib/api";
import { authStorageKey, practiceSources } from "@/lib/constants";
import { practiceModes } from "@/lib/practiceModes";
import { CheckCircle2, Code2, Mic2, Sparkles } from "lucide-react";
import { FormEvent, useEffect, useMemo, useState } from "react";

type AuthMode = "login" | "register";

const suggestions = [
  {
    icon: Code2,
    title: "Developer contexts",
    label: "API design, debugging, code review",
  },
  {
    icon: Mic2,
    title: "Phase 2 ready",
    label: "Voice input and pronunciation scoring",
  },
  {
    icon: CheckCircle2,
    title: "Feedback loop",
    label: "Grammar, natural wording, confidence",
  },
];

export default function Home() {
  const [mode, setMode] = useState<PracticeMode>("chat");
  const [message, setMessage] = useState("");
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [auth, setAuth] = useState<AuthResponse | null>(null);
  const [authMode, setAuthMode] = useState<AuthMode>("login");
  const [progress, setProgress] = useState<UserProgress | null>(null);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [authError, setAuthError] = useState("");
  const [statusError, setStatusError] = useState("");
  const [backendStatus, setBackendStatus] = useState<BackendStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isAuthLoading, setIsAuthLoading] = useState(false);
  const [isHistoryLoading, setIsHistoryLoading] = useState(false);
  const [isProgressLoading, setIsProgressLoading] = useState(false);
  const [isHistoryClearing, setIsHistoryClearing] = useState(false);
  const [isStatusLoading, setIsStatusLoading] = useState(false);
  const [focusedHistoryItemId, setFocusedHistoryItemId] = useState("");

  const activeMode = useMemo(
    () => practiceModes.find((item) => item.id === mode) ?? practiceModes[0],
    [mode],
  );

  useEffect(() => {
    void loadBackendStatus();

    const storedAuth = localStorage.getItem(authStorageKey);

    if (storedAuth) {
      try {
        const parsedAuth = JSON.parse(storedAuth) as AuthResponse;
        setAuth(parsedAuth);
        void validateStoredAuth(parsedAuth);
      } catch {
        localStorage.removeItem(authStorageKey);
      }
    }
  }, []);

  async function loadBackendStatus() {
    if (!hasApiBaseUrl()) {
      setStatusError("Backend API URL is required.");
      return;
    }

    setIsStatusLoading(true);
    setStatusError("");

    try {
      setBackendStatus(await getBackendStatus());
    } catch (requestError) {
      setBackendStatus(null);
      setStatusError(
        requestError instanceof Error
          ? requestError.message
          : "Could not reach backend.",
      );
    } finally {
      setIsStatusLoading(false);
    }
  }

  async function validateStoredAuth(storedAuth: AuthResponse) {
    if (!hasApiBaseUrl()) {
      return;
    }

    try {
      const profile = await getProfile(storedAuth.token);
      const refreshedAuth = {
        ...storedAuth,
        userId: profile.userId,
        email: profile.email,
      };

      localStorage.setItem(
        authStorageKey,
        JSON.stringify(refreshedAuth),
      );
      setAuth(refreshedAuth);
    } catch {
      handleLogout();
    }
  }

  useEffect(() => {
    if (!auth) {
      return;
    }

    void loadHistory(auth.token);
    void loadProgress(auth.token);
  }, [auth]);

  async function loadHistory(token: string) {
    if (!hasApiBaseUrl()) {
      return;
    }

    try {
      setIsHistoryLoading(true);
      const data = await getHistory(token);
      setHistory(
        data.map((item) => ({
          ...item,
          createdAt: formatCreatedAt(item.createdAt),
        })),
      );
    } catch (requestError) {
      handleRequestError(requestError, "Could not load saved practice history.");
    } finally {
      setIsHistoryLoading(false);
    }
  }

  async function loadProgress(token: string) {
    if (!hasApiBaseUrl()) {
      return;
    }

    try {
      setIsProgressLoading(true);
      setProgress(await getProgress(token));
    } catch (requestError) {
      handleRequestError(requestError, "Could not load progress.");
    } finally {
      setIsProgressLoading(false);
    }
  }

  async function handleAuth(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!hasApiBaseUrl()) {
      setAuthError("Backend API URL is required for authentication.");
      return;
    }

    setIsAuthLoading(true);
    setAuthError("");

    try {
      const data =
        authMode === "login"
          ? await login(email, password)
          : await register(email, password);

      localStorage.setItem(authStorageKey, JSON.stringify(data));
      setAuth(data);
      setPassword("");
      setError("");
    } catch (requestError) {
      setAuthError(
        requestError instanceof Error
          ? requestError.message
          : "Authentication failed.",
      );
    } finally {
      setIsAuthLoading(false);
    }
  }

  function handleLogout() {
    localStorage.removeItem(authStorageKey);
    setAuth(null);
    setHistory([]);
    setProgress(null);
    setPassword("");
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const submittedMessage = message.trim();

    if (!submittedMessage) {
      return;
    }

    setIsLoading(true);
    setError("");

    try {
      if (!auth?.token) {
        throw new Error("Please sign in before practicing.");
      }

      const data = await submitPractice(auth.token, mode, submittedMessage);

      if (!data.feedback) {
        throw new Error("Feedback was empty.");
      }

      if (data.source === practiceSources.localFallback) {
        setError(
          "AI provider is unavailable. Showing local fallback feedback for now.",
        );
      }

      const feedback = data.feedback;
      const savedAttempt = data.attempt;
      const nextItem = savedAttempt
        ? {
            ...savedAttempt,
            createdAt: formatCreatedAt(savedAttempt.createdAt),
          }
        : {
            id: crypto.randomUUID(),
            mode,
            message: submittedMessage,
            feedback,
            source: data.source ?? practiceSources.openAi,
            createdAt: formatCreatedAt(new Date().toISOString()),
          };

      setHistory((currentHistory) => {
        return [
          nextItem,
          ...currentHistory.filter((item) => item.id !== nextItem.id),
        ];
      });
      setFocusedHistoryItemId(nextItem.id);
      void loadHistory(auth.token);
      void loadProgress(auth.token);
      void loadBackendStatus();
      setMessage("");
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Could not generate feedback.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  async function handleClearHistory() {
    if (!auth?.token || history.length === 0) {
      return;
    }

    const shouldClear = window.confirm(
      "Clear all saved practice history for this account?",
    );

    if (!shouldClear) {
      return;
    }

    setIsHistoryClearing(true);
    setError("");

    try {
      await clearHistory(auth.token);
      setHistory([]);
      await loadProgress(auth.token);
    } catch (requestError) {
      handleRequestError(requestError, "Could not clear practice history.");
    } finally {
      setIsHistoryClearing(false);
    }
  }

  function handleRequestError(requestError: unknown, fallbackMessage: string) {
    if (requestError instanceof ApiRequestError && requestError.status === 401) {
      handleLogout();
      setAuthError("Your session expired. Please sign in again.");
      return;
    }

    setError(
      requestError instanceof Error ? requestError.message : fallbackMessage,
    );
  }

  return (
    <main className="app-shell">
      <div className="workspace">
        <aside className="sidebar" aria-label="Product status">
          <div className="brand">
            <div className="brand-mark" aria-hidden="true">
              <Sparkles size={24} />
            </div>
            <div>
              <h1>English for Developers</h1>
              <p>AI-powered English learning for software engineers.</p>
            </div>
          </div>

          <p className="section-label">MVP focus</p>
          <div className="metrics">
            {suggestions.map((item) => {
              const Icon = item.icon;
              return (
                <div className="metric" key={item.title}>
                  <Icon size={22} aria-hidden="true" />
                  <div>
                    <strong>{item.title}</strong>
                    <span>{item.label}</span>
                  </div>
                </div>
              );
            })}
          </div>

          <p className="section-label">Account</p>
          <AuthPanel
            auth={auth}
            authMode={authMode}
            email={email}
            password={password}
            error={authError}
            isLoading={isAuthLoading}
            onAuthModeChange={setAuthMode}
            onEmailChange={setEmail}
            onPasswordChange={setPassword}
            onSubmit={handleAuth}
            onLogout={handleLogout}
          />

          {auth ? (
            <ProgressPanel
              progress={progress}
              isLoading={isProgressLoading}
              onRefresh={() => {
                if (auth.token) {
                  void loadProgress(auth.token);
                }
              }}
            />
          ) : null}

          <p className="section-label">System</p>
          <StatusPanel
            apiBaseUrl={getApiBaseUrl()}
            status={backendStatus}
            error={statusError}
            isLoading={isStatusLoading}
            onRefresh={() => {
              void loadBackendStatus();
            }}
          />

          <p className="section-label">Roadmap</p>
          <div className="metrics">
            <div className="metric">
              <CheckCircle2 size={22} aria-hidden="true" />
              <div>
                <strong>Phase 1</strong>
                <span>Auth, AI feedback, saved history, progress</span>
              </div>
            </div>
            <div className="metric">
              <Mic2 size={22} aria-hidden="true" />
              <div>
                <strong>Phase 2</strong>
                <span>Voice input, pronunciation, interviews</span>
              </div>
            </div>
          </div>
        </aside>

        <section className="main" aria-label="English practice workspace">
          <div className="hero">
            <h2>Practice the English you need at work.</h2>
            <p>
              Prepare for standups, interviews, code reviews, and architecture
              discussions with feedback tuned for engineering communication.
            </p>
          </div>

          <ModeSelector
            mode={mode}
            modes={practiceModes}
            onModeChange={(nextMode) => {
              setMode(nextMode);
              setMessage("");
              setError("");
            }}
          />

          <PracticeComposer
            activeMode={activeMode}
            message={message}
            isLoading={isLoading}
            isAuthenticated={Boolean(auth)}
            onMessageChange={setMessage}
            onSubmit={handleSubmit}
          />

          <VocabularyPanel
            history={history}
            onPracticePhrase={(phrase) => {
              setMessage(`Can you help me use "${phrase}" in a developer work sentence?`);
              setError("");
            }}
          />

          <HistoryList
            history={history}
            modes={practiceModes}
            error={error}
            isAuthenticated={Boolean(auth)}
            isLoading={isHistoryLoading}
            isClearing={isHistoryClearing}
            focusedItemId={focusedHistoryItemId}
            onRefresh={() => {
              if (auth?.token) {
                void loadHistory(auth.token);
              }
            }}
            onClear={() => {
              void handleClearHistory();
            }}
          />
        </section>
      </div>
    </main>
  );
}

function formatCreatedAt(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });
}
