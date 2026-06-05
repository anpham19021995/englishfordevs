export type PracticeMode = "chat" | "interview" | "converter";

export type PracticeFeedback = {
  directReply: string;
  correctedVersion: string;
  naturalVersion: string;
  vocabulary: string[];
  confidenceFeedback: string;
  followUpQuestion: string;
};

export type HistoryItem = {
  id: string;
  mode: PracticeMode;
  message: string;
  feedback: PracticeFeedback;
  source: string;
  createdAt: string;
};

export type AuthResponse = {
  token: string;
  userId: string;
  email: string;
  expiresAt: string;
};

export type UserProgress = {
  totalPractices: number;
  chatPractices: number;
  interviewPractices: number;
  converterPractices: number;
  currentStreakDays: number;
  lastPracticeAt: string | null;
};

export type UserProfile = {
  userId: string;
  email: string;
  createdAt: string | null;
};

type PracticeResponse = {
  feedback?: PracticeFeedback;
  source?: string;
  attempt?: HistoryItem;
  error?: string;
};

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "");

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "ApiRequestError";
  }
}

export function hasApiBaseUrl() {
  return Boolean(apiBaseUrl);
}

export async function login(email: string, password: string) {
  return sendAuthRequest("login", email, password);
}

export async function register(email: string, password: string) {
  return sendAuthRequest("register", email, password);
}

export async function getProfile(token: string) {
  return request<UserProfile>("/api/me", {
    headers: createAuthHeaders(token),
  });
}

export async function getProgress(token: string) {
  return request<UserProgress>("/api/me/progress", {
    headers: createAuthHeaders(token),
  });
}

export async function getHistory(token: string, take = 20) {
  return request<HistoryItem[]>(`/api/practice/history?take=${take}`, {
    headers: createAuthHeaders(token),
  });
}

export async function submitPractice(
  token: string,
  mode: PracticeMode,
  message: string,
) {
  return request<PracticeResponse>("/api/practice", {
    method: "POST",
    headers: createAuthHeaders(token),
    body: JSON.stringify({ mode, message }),
  });
}

export async function clearHistory(token: string) {
  return request<{ deletedCount: number }>("/api/practice/history", {
    method: "DELETE",
    headers: createAuthHeaders(token),
  });
}

function createAuthHeaders(token?: string) {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function sendAuthRequest(
  action: "login" | "register",
  email: string,
  password: string,
) {
  return request<AuthResponse>(`/api/auth/${action}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
}

async function request<T>(path: string, init?: RequestInit) {
  if (!apiBaseUrl) {
    throw new Error("NEXT_PUBLIC_API_BASE_URL is required.");
  }

  const response = await fetch(`${apiBaseUrl}${path}`, init);
  const data = (await response.json().catch(() => ({}))) as T & {
    error?: string;
  };

  if (!response.ok) {
    throw new ApiRequestError(
      data.error ?? defaultErrorForStatus(response.status),
      response.status,
    );
  }

  return data;
}

function defaultErrorForStatus(status: number) {
  if (status === 401) {
    return "Please sign in before practicing.";
  }

  return "Request failed.";
}
