import type { UserProgress } from "@/lib/api";
import { RotateCw, Trophy } from "lucide-react";

type ProgressPanelProps = {
  progress: UserProgress | null;
  isLoading: boolean;
  onRefresh: () => void;
};

export function ProgressPanel({
  progress,
  isLoading,
  onRefresh,
}: ProgressPanelProps) {
  if (!progress && !isLoading) {
    return null;
  }

  return (
    <>
      <p className="section-label">Progress</p>
      <div className="metrics compact-metrics">
        <div className="metric">
          <Trophy size={22} aria-hidden="true" />
          <div>
            <strong>
              {progress ? `${progress.totalPractices} practices` : "Loading"}
            </strong>
            <span>
              {progress ? `${progress.currentStreakDays} day streak` : "Syncing progress"}
            </span>
          </div>
        </div>
        <div className="mode-stats with-action" aria-label="Practice mode counts">
          <span>Chat {progress?.chatPractices ?? 0}</span>
          <span>Interview {progress?.interviewPractices ?? 0}</span>
          <span>VN {progress?.converterPractices ?? 0}</span>
          <button
            className="icon-button"
            type="button"
            aria-label="Refresh progress"
            title="Refresh progress"
            onClick={onRefresh}
            disabled={isLoading}
          >
            <RotateCw size={15} aria-hidden="true" />
          </button>
        </div>
      </div>
    </>
  );
}
