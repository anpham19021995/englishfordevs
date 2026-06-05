import {
  BriefcaseBusiness,
  Languages,
  MessageSquareText,
  type LucideIcon,
} from "lucide-react";
import type { PracticeMode } from "./api";

export type PracticeModeOption = {
  id: PracticeMode;
  title: string;
  description: string;
  placeholder: string;
  icon: LucideIcon;
};

export const practiceModes: PracticeModeOption[] = [
  {
    id: "chat",
    title: "AI Chat Practice",
    description: "Practice standups, task updates, blockers, and teammate conversations.",
    placeholder:
      "Example: I fixed bug in payment API but I am not sure how to explain root cause clearly.",
    icon: MessageSquareText,
  },
  {
    id: "interview",
    title: "Mock Interview",
    description: "Answer backend interview questions and improve technical English.",
    placeholder:
      "Example: Ask me about API design, scalability, and how I communicate trade-offs.",
    icon: BriefcaseBusiness,
  },
  {
    id: "converter",
    title: "VN -> Professional English",
    description: "Turn Vietnamese IT explanations into natural engineering English.",
    placeholder:
      "Example: Service nay xu ly request bat dong bo va day message vao queue cho service khac.",
    icon: Languages,
  },
];
