import type { AuthResponse } from "@/lib/api";
import { validationLimits } from "@/lib/constants";
import { LogOut } from "lucide-react";
import type { FormEvent } from "react";

type AuthMode = "login" | "register";

type AuthPanelProps = {
  auth: AuthResponse | null;
  authMode: AuthMode;
  email: string;
  password: string;
  error: string;
  isLoading: boolean;
  onAuthModeChange: (mode: AuthMode) => void;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onLogout: () => void;
};

export function AuthPanel({
  auth,
  authMode,
  email,
  password,
  error,
  isLoading,
  onAuthModeChange,
  onEmailChange,
  onPasswordChange,
  onSubmit,
  onLogout,
}: AuthPanelProps) {
  if (auth) {
    return (
      <div className="auth-card">
        <div>
          <strong>{auth.email}</strong>
          <span>History is saved to your account.</span>
        </div>
        <button className="secondary-button" type="button" onClick={onLogout}>
          <LogOut size={16} aria-hidden="true" />
          Sign out
        </button>
      </div>
    );
  }

  return (
    <form className="auth-card" onSubmit={onSubmit}>
      <div className="auth-toggle" role="group" aria-label="Authentication mode">
        <button
          type="button"
          aria-pressed={authMode === "login"}
          onClick={() => onAuthModeChange("login")}
        >
          Login
        </button>
        <button
          type="button"
          aria-pressed={authMode === "register"}
          onClick={() => onAuthModeChange("register")}
        >
          Register
        </button>
      </div>
      <input
        aria-label="Email"
        autoComplete="email"
        type="email"
        value={email}
        maxLength={validationLimits.emailMaxLength}
        placeholder="you@example.com"
        onChange={(event) => onEmailChange(event.target.value)}
      />
      <input
        aria-label="Password"
        autoComplete={authMode === "login" ? "current-password" : "new-password"}
        type="password"
        value={password}
        maxLength={validationLimits.passwordMaxLength}
        placeholder="Password"
        onChange={(event) => onPasswordChange(event.target.value)}
      />
      {error ? <p className="error compact">{error}</p> : null}
      <button
        className="primary-button full-width"
        type="submit"
        disabled={
          isLoading ||
          !email.trim() ||
          password.length < validationLimits.passwordMinLength
        }
      >
        {isLoading ? "Working" : authMode === "login" ? "Login" : "Create account"}
      </button>
    </form>
  );
}
