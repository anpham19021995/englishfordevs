import type { HistoryItem } from "@/lib/api";
import type { PracticeModeOption } from "@/lib/practiceModes";
import { History, RotateCw, Trash2 } from "lucide-react";

type HistoryListProps = {
  history: HistoryItem[];
  modes: PracticeModeOption[];
  error: string;
  isAuthenticated: boolean;
  isLoading: boolean;
  isClearing: boolean;
  onRefresh: () => void;
  onClear: () => void;
};

export function HistoryList({
  history,
  modes,
  error,
  isAuthenticated,
  isLoading,
  isClearing,
  onRefresh,
  onClear,
}: HistoryListProps) {
  return (
    <section className="output-panel" aria-live="polite">
      <div className="practice-header">
        <h3>Session history</h3>
        <div className="header-actions">
          <span className="status-pill">
            <History size={16} aria-hidden="true" />
            {isLoading
              ? "Loading"
              : `${history.length} ${history.length === 1 ? "turn" : "turns"}`}
          </span>
          {isAuthenticated ? (
            <>
              <button
                className="icon-button danger-icon-button"
                type="button"
                aria-label="Clear history"
                title="Clear history"
                onClick={onClear}
                disabled={isLoading || isClearing || history.length === 0}
              >
                <Trash2 size={16} aria-hidden="true" />
              </button>
              <button
                className="icon-button"
                type="button"
                aria-label="Refresh history"
                title="Refresh history"
                onClick={onRefresh}
                disabled={isLoading || isClearing}
              >
                <RotateCw size={16} aria-hidden="true" />
              </button>
            </>
          ) : null}
        </div>
      </div>

      {error ? <p className="error">{error}</p> : null}
      {!isAuthenticated ? (
        <div className="empty-state">
          Sign in to save and review your practice history.
        </div>
      ) : null}
      {isAuthenticated && history.length > 0 ? (
        <div className="history-list">
          {history.map((item) => {
            const modeTitle =
              modes.find((modeItem) => modeItem.id === item.mode)?.title ??
              "Practice";

            return (
              <article className="history-item" key={item.id}>
                <header className="history-header">
                  <div>
                    <strong>{modeTitle}</strong>
                    <p>
                      {item.createdAt} -{" "}
                      {formatSource(item.source)}
                    </p>
                  </div>
                </header>

                <div className="history-prompt">
                  <span>Your text</span>
                  <p>{item.message}</p>
                </div>

                <div className="feedback-grid">
                  <FeedbackBlock
                    title="Direct reply"
                    value={item.feedback.directReply}
                  />
                  <FeedbackBlock
                    title="Corrected version"
                    value={item.feedback.correctedVersion}
                  />
                  <FeedbackBlock
                    title="Natural version"
                    value={item.feedback.naturalVersion}
                  />
                  <FeedbackBlock
                    title="Confidence"
                    value={item.feedback.confidenceFeedback}
                  />
                  <FeedbackBlock
                    title="Follow-up question"
                    value={item.feedback.followUpQuestion}
                  />
                </div>

                <div className="vocabulary-list" aria-label="Vocabulary">
                  {item.feedback.vocabulary.map((word) => (
                    <VocabularyChip key={word} value={word} />
                  ))}
                </div>
              </article>
            );
          })}
        </div>
      ) : isAuthenticated && isLoading ? (
        <div className="empty-state">Loading your saved practice history.</div>
      ) : isAuthenticated ? (
        <div className="empty-state">
          Your corrected wording, interview coaching, or conversion will appear
          here.
        </div>
      ) : null}
    </section>
  );
}

function formatSource(source: string) {
  if (source === "openai") {
    return "OpenAI";
  }

  if (source === "ollama") {
    return "Ollama";
  }

  return "local fallback";
}

function FeedbackBlock({ title, value }: { title: string; value: string }) {
  return (
    <div className="feedback-block">
      <span>{title}</span>
      <p>{value}</p>
    </div>
  );
}

function VocabularyChip({ value }: { value: string }) {
  const [phrase, meaning] = value.split(" - ", 2);

  return (
    <span>
      <strong>{phrase}</strong>
      {meaning ? <small>{meaning}</small> : null}
    </span>
  );
}
