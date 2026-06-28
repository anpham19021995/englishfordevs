"use client";

import type { HistoryItem } from "@/lib/api";
import {
  developerVocabulary,
  vocabularyCategories,
  type DeveloperVocabularyItem,
  type VocabularyCategory,
} from "@/lib/developerVocabulary";
import { BookOpenText, Lightbulb, Search, Send } from "lucide-react";
import { useMemo, useState } from "react";

type VocabularyPanelProps = {
  history: HistoryItem[];
  onPracticePhrase: (phrase: string) => void;
};

type VocabularyMode = "top" | "ai" | "history";
type CategoryFilter = "all" | VocabularyCategory;
type DerivedVocabularyItem = DeveloperVocabularyItem & {
  seenCount?: number;
  sourceLabel?: string;
};

export function VocabularyPanel({
  history,
  onPracticePhrase,
}: VocabularyPanelProps) {
  const [mode, setMode] = useState<VocabularyMode>("top");
  const [category, setCategory] = useState<CategoryFilter>("all");
  const [query, setQuery] = useState("");

  const latestAiVocabulary = useMemo(() => deriveLatestAiVocabulary(history), [history]);
  const historyVocabulary = useMemo(() => deriveVocabularyFromHistory(history), [history]);
  const items = useMemo(() => {
    if (mode === "ai") {
      return latestAiVocabulary;
    }

    if (mode === "history") {
      return historyVocabulary;
    }

    return developerVocabulary;
  }, [historyVocabulary, latestAiVocabulary, mode]);
  const filteredItems = useMemo(
    () => filterVocabulary(items, category, query),
    [items, category, query],
  );
  const modeSummary = getModeSummary(mode, {
    topCount: developerVocabulary.length,
    aiCount: latestAiVocabulary.length,
    historyCount: historyVocabulary.length,
  });

  return (
    <section className="vocabulary-panel" aria-labelledby="vocabulary-title">
      <div className="practice-header">
        <div>
          <h3 id="vocabulary-title">Vocabulary Review</h3>
          <p className="panel-subtitle">
            Review core phrases, latest AI suggestions, and words from your practice history.
          </p>
        </div>
        <span className="status-pill">
          <BookOpenText size={16} aria-hidden="true" />
          {modeSummary.countLabel}
        </span>
      </div>

      <div className="vocabulary-toolbar">
        <div className="segmented-control vocabulary-source-control" role="group" aria-label="Vocabulary source">
          <button
            type="button"
            aria-pressed={mode === "top"}
            onClick={() => setMode("top")}
          >
            Top
          </button>
          <button
            type="button"
            aria-pressed={mode === "ai"}
            onClick={() => setMode("ai")}
          >
            Latest AI
          </button>
          <button
            type="button"
            aria-pressed={mode === "history"}
            onClick={() => setMode("history")}
          >
            History
          </button>
        </div>

        <label className="search-field">
          <Search size={16} aria-hidden="true" />
          <input
            aria-label="Search vocabulary"
            value={query}
            placeholder="Search phrase"
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>

        <label className="category-filter">
          <span>Category</span>
          <select
            value={category}
            onChange={(event) => setCategory(event.target.value as CategoryFilter)}
          >
            <option value="all">All categories</option>
            {vocabularyCategories.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="vocabulary-source-summary">
        <Lightbulb size={16} aria-hidden="true" />
        <p>{modeSummary.description}</p>
      </div>

      {filteredItems.length > 0 ? (
        <div className="vocabulary-grid">
          {filteredItems.map((item) => (
            <article className="vocabulary-card" key={`${item.category}-${item.phrase}`}>
              <div>
                <span>{item.category}{item.sourceLabel ? ` / ${item.sourceLabel}` : ""}</span>
                <h4>{item.phrase}</h4>
              </div>
              <p>{item.meaning}</p>
              <small>{item.example}</small>
              <div className="vocabulary-card-footer">
                <strong>{getItemLabel(item, mode)}</strong>
                <button
                  className="secondary-button"
                  type="button"
                  onClick={() => onPracticePhrase(item.phrase)}
                >
                  <Send size={15} aria-hidden="true" />
                  Practice
                </button>
              </div>
            </article>
          ))}
        </div>
      ) : (
        <div className="empty-state">
          {mode === "ai"
            ? "Submit feedback once to see AI-suggested vocabulary from your latest turn."
            : mode === "history"
              ? "Submit practice feedback to build your personal vocabulary list."
            : "No vocabulary matches the selected filters."}
        </div>
      )}
    </section>
  );
}

function deriveLatestAiVocabulary(history: HistoryItem[]) {
  const latestAttempt = history.find((attempt) => attempt.feedback.vocabulary.length > 0);

  if (!latestAttempt) {
    return [];
  }

  return latestAttempt.feedback.vocabulary
    .map((value) => parseVocabularyItem(value))
    .filter((item): item is DerivedVocabularyItem => Boolean(item))
    .map((item) => ({
      ...item,
      sourceLabel: "latest feedback",
    }));
}

function deriveVocabularyFromHistory(history: HistoryItem[]) {
  const vocabulary = new Map<string, DerivedVocabularyItem>();

  for (const attempt of history) {
    for (const value of attempt.feedback.vocabulary) {
      const item = parseVocabularyItem(value);

      if (!item) {
        continue;
      }

      const key = item.phrase.toLowerCase();
      const existing = vocabulary.get(key);

      vocabulary.set(key, {
        ...item,
        seenCount: (existing?.seenCount ?? 0) + 1,
        sourceLabel: "saved history",
      });
    }
  }

  return Array.from(vocabulary.values()).sort(
    (left, right) =>
      (right.seenCount ?? 0) - (left.seenCount ?? 0) ||
      left.phrase.localeCompare(right.phrase),
  );
}

function parseVocabularyItem(value: string): DerivedVocabularyItem | null {
  const trimmedValue = value.trim();

  if (!trimmedValue) {
    return null;
  }

  const [rawPhrase, rawMeaning] = trimmedValue.split(" - ", 2);
  const phrase = rawPhrase.trim();

  if (!phrase) {
    return null;
  }

  const knownItem = developerVocabulary.find(
    (item) => item.phrase.toLowerCase() === phrase.toLowerCase(),
  );

  if (knownItem) {
    return knownItem;
  }

  const meaning = rawMeaning?.trim() || "A useful phrase from your AI feedback.";

  return {
    phrase,
    meaning,
    example: `Try using "${phrase}" in your next technical update.`,
    category: inferCategory(phrase),
  };
}

function inferCategory(phrase: string): VocabularyCategory {
  const value = phrase.toLowerCase();

  if (value.includes("incident") || value.includes("rollback") || value.includes("mitigation")) {
    return "Incident";
  }

  if (value.includes("review") || value.includes("merge") || value.includes("test")) {
    return "Code Review";
  }

  if (value.includes("scale") || value.includes("architecture") || value.includes("trade")) {
    return "Architecture";
  }

  if (value.includes("interview") || value.includes("assumption")) {
    return "Interview";
  }

  if (value.includes("follow") || value.includes("align") || value.includes("blocker")) {
    return "Meetings";
  }

    return "Debugging";
}

function getModeSummary(
  mode: VocabularyMode,
  counts: {
    topCount: number;
    aiCount: number;
    historyCount: number;
  },
) {
  if (mode === "ai") {
    return {
      countLabel: `${counts.aiCount} suggested`,
      description:
        "Latest AI shows phrases from your most recent feedback, so you can review the vocabulary while the context is still fresh.",
    };
  }

  if (mode === "history") {
    return {
      countLabel: `${counts.historyCount} saved`,
      description:
        "History collects vocabulary from previous feedback and sorts repeated phrases first.",
    };
  }

  return {
    countLabel: `${counts.topCount} phrases`,
    description:
      "Top vocabulary is a stable starter set for debugging, code review, meetings, incidents, architecture, and interviews.",
  };
}

function getItemLabel(item: DerivedVocabularyItem, mode: VocabularyMode) {
  if (mode === "history" && item.seenCount) {
    return `Seen ${item.seenCount}x`;
  }

  if (mode === "ai") {
    return "AI suggested";
  }

  return "Core phrase";
}

function filterVocabulary(
  items: DerivedVocabularyItem[],
  category: CategoryFilter,
  query: string,
) {
  const normalizedQuery = query.trim().toLowerCase();

  return items.filter((item) => {
    const matchesCategory = category === "all" || item.category === category;
    const matchesQuery =
      !normalizedQuery ||
      item.phrase.toLowerCase().includes(normalizedQuery) ||
      item.meaning.toLowerCase().includes(normalizedQuery) ||
      item.example.toLowerCase().includes(normalizedQuery);

    return matchesCategory && matchesQuery;
  });
}
