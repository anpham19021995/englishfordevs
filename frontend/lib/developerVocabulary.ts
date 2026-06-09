export type VocabularyCategory =
  | "Debugging"
  | "Code Review"
  | "Meetings"
  | "Interview"
  | "Incident"
  | "Architecture";

export type DeveloperVocabularyItem = {
  phrase: string;
  meaning: string;
  example: string;
  category: VocabularyCategory;
};

export const vocabularyCategories: VocabularyCategory[] = [
  "Debugging",
  "Code Review",
  "Meetings",
  "Interview",
  "Incident",
  "Architecture",
];

export const developerVocabulary: DeveloperVocabularyItem[] = [
  {
    phrase: "root cause",
    meaning: "The real underlying reason a problem happened.",
    example: "The root cause was a missing validation check in the payment flow.",
    category: "Debugging",
  },
  {
    phrase: "reproduce the issue",
    meaning: "Make the problem happen again so it can be investigated.",
    example: "I can reproduce the issue when the request payload is empty.",
    category: "Debugging",
  },
  {
    phrase: "edge case",
    meaning: "An unusual input or condition that can break normal behavior.",
    example: "This edge case happens when the user has no saved preferences.",
    category: "Debugging",
  },
  {
    phrase: "regression",
    meaning: "A bug where something that worked before stops working.",
    example: "The latest release introduced a regression in the login flow.",
    category: "Debugging",
  },
  {
    phrase: "workaround",
    meaning: "A temporary way to avoid a problem before the real fix is ready.",
    example: "As a workaround, we can retry the request after refreshing the token.",
    category: "Debugging",
  },
  {
    phrase: "blocker",
    meaning: "Something that prevents progress.",
    example: "The missing API contract is currently a blocker for frontend integration.",
    category: "Meetings",
  },
  {
    phrase: "follow up",
    meaning: "Continue or check something after the current discussion.",
    example: "I will follow up with the backend team after standup.",
    category: "Meetings",
  },
  {
    phrase: "align on",
    meaning: "Agree on the same direction or decision.",
    example: "We need to align on the rollout plan before merging this change.",
    category: "Meetings",
  },
  {
    phrase: "scope creep",
    meaning: "When a task keeps growing beyond the original plan.",
    example: "This ticket has some scope creep, so I split the extra work into a new task.",
    category: "Meetings",
  },
  {
    phrase: "trade-off",
    meaning: "A decision where you gain one benefit but accept another cost.",
    example: "The trade-off is faster delivery but less flexibility later.",
    category: "Architecture",
  },
  {
    phrase: "single point of failure",
    meaning: "One component that can break the whole system if it fails.",
    example: "The cache server is a single point of failure in the current design.",
    category: "Architecture",
  },
  {
    phrase: "event-driven",
    meaning: "A design where services react to events instead of direct calls.",
    example: "An event-driven approach would reduce coupling between services.",
    category: "Architecture",
  },
  {
    phrase: "backward compatible",
    meaning: "Still works with older clients or previous versions.",
    example: "This API change is backward compatible because the old field still works.",
    category: "Architecture",
  },
  {
    phrase: "bottleneck",
    meaning: "The slowest part that limits overall performance.",
    example: "Database writes are the main bottleneck during peak traffic.",
    category: "Architecture",
  },
  {
    phrase: "nitpick",
    meaning: "A small non-blocking review comment.",
    example: "Nitpick: we could rename this variable to make it clearer.",
    category: "Code Review",
  },
  {
    phrase: "non-blocking",
    meaning: "Useful feedback that should not stop the change from merging.",
    example: "This is non-blocking, but the helper name could be more specific.",
    category: "Code Review",
  },
  {
    phrase: "looks good to me",
    meaning: "A common approval phrase in code review.",
    example: "Looks good to me after the test update.",
    category: "Code Review",
  },
  {
    phrase: "address feedback",
    meaning: "Make changes based on review comments.",
    example: "I addressed the feedback and pushed a smaller commit.",
    category: "Code Review",
  },
  {
    phrase: "test coverage",
    meaning: "How much behavior is checked by tests.",
    example: "I added test coverage for the expired-token case.",
    category: "Code Review",
  },
  {
    phrase: "incident",
    meaning: "A production problem that affects users or service health.",
    example: "We opened an incident after the API error rate increased.",
    category: "Incident",
  },
  {
    phrase: "mitigation",
    meaning: "An action that reduces impact before the full fix is done.",
    example: "The mitigation was to disable the new cache path temporarily.",
    category: "Incident",
  },
  {
    phrase: "rollback",
    meaning: "Revert to a previous version.",
    example: "We decided to rollback the release after seeing database timeouts.",
    category: "Incident",
  },
  {
    phrase: "postmortem",
    meaning: "A review after an incident to learn what happened.",
    example: "The postmortem identified weak monitoring around queue latency.",
    category: "Incident",
  },
  {
    phrase: "blast radius",
    meaning: "How much of the system or user base is affected by a failure.",
    example: "Feature flags helped reduce the blast radius of the deployment.",
    category: "Incident",
  },
  {
    phrase: "clarify assumptions",
    meaning: "Explain what you believe is true before solving a problem.",
    example: "I would clarify assumptions about traffic volume before choosing a database.",
    category: "Interview",
  },
  {
    phrase: "walk through",
    meaning: "Explain step by step.",
    example: "Let me walk through how I would debug this issue.",
    category: "Interview",
  },
  {
    phrase: "scale horizontally",
    meaning: "Add more machines or instances instead of making one machine bigger.",
    example: "We can scale horizontally by adding more API replicas behind the load balancer.",
    category: "Interview",
  },
  {
    phrase: "failure mode",
    meaning: "A way a system can fail.",
    example: "One failure mode is that the queue grows faster than workers can process it.",
    category: "Interview",
  },
  {
    phrase: "justify the decision",
    meaning: "Explain why a choice is reasonable.",
    example: "I would justify the decision by comparing complexity, cost, and reliability.",
    category: "Interview",
  },
];
