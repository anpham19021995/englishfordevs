import type { PracticeMode } from "@/lib/api";
import type { PracticeModeOption } from "@/lib/practiceModes";

type ModeSelectorProps = {
  mode: PracticeMode;
  modes: PracticeModeOption[];
  onModeChange: (mode: PracticeMode) => void;
};

export function ModeSelector({ mode, modes, onModeChange }: ModeSelectorProps) {
  return (
    <div className="mode-grid" role="group" aria-label="Practice modes">
      {modes.map((item) => {
        const Icon = item.icon;
        return (
          <button
            className="mode"
            key={item.id}
            type="button"
            aria-pressed={mode === item.id}
            onClick={() => onModeChange(item.id)}
          >
            <Icon size={24} aria-hidden="true" />
            <strong>{item.title}</strong>
            <p>{item.description}</p>
          </button>
        );
      })}
    </div>
  );
}
