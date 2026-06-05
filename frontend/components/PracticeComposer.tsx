import { validationLimits } from "@/lib/constants";
import type { PracticeModeOption } from "@/lib/practiceModes";
import { Send, Sparkles } from "lucide-react";
import type { FormEvent } from "react";

type PracticeComposerProps = {
  activeMode: PracticeModeOption;
  message: string;
  isLoading: boolean;
  isAuthenticated: boolean;
  onMessageChange: (value: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
};

export function PracticeComposer({
  activeMode,
  message,
  isLoading,
  isAuthenticated,
  onMessageChange,
  onSubmit,
}: PracticeComposerProps) {
  return (
    <section className="practice-panel" aria-labelledby="practice-title">
      <div className="practice-header">
        <h3 id="practice-title">{activeMode.title}</h3>
        <span className="status-pill">
          <Sparkles size={16} aria-hidden="true" />
          AI feedback
        </span>
      </div>

      <form className="composer" onSubmit={onSubmit}>
        <textarea
          aria-label="Practice message"
          value={message}
          placeholder={activeMode.placeholder}
          maxLength={validationLimits.practiceMessageMaxLength}
          onChange={(event) => onMessageChange(event.target.value)}
        />
        <div className="actions">
          <p className="hint">
            Write naturally. {message.length}/
            {validationLimits.practiceMessageMaxLength}
          </p>
          <button
            className="primary-button"
            type="submit"
            disabled={isLoading || !message.trim() || !isAuthenticated}
          >
            <Send size={18} aria-hidden="true" />
            {isLoading ? "Reviewing" : "Get feedback"}
          </button>
        </div>
      </form>
    </section>
  );
}
