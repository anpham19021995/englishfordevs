export const authStorageKey = "english-for-devs-auth";

export const aiProviders = {
  openAi: "openai",
  ollama: "ollama",
} as const;

export const practiceSources = {
  openAi: aiProviders.openAi,
  ollama: aiProviders.ollama,
  localFallback: "local-fallback",
} as const;

export const historyStorageTypes = {
  inMemory: "in-memory",
  postgres: "postgres",
} as const;

export const validationLimits = {
  emailMaxLength: 256,
  passwordMinLength: 8,
  passwordMaxLength: 128,
  practiceMessageMaxLength: 4000,
} as const;
