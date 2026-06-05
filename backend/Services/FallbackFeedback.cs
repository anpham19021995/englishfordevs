namespace EnglishForDevs.Api.Services;

public static class FallbackFeedback
{
    public static PracticeFeedback ForMode(string mode)
    {
        return mode switch
        {
            PracticeModes.Interview => new PracticeFeedback(
                "Good start. In an interview, answer with context, trade-off, and impact.",
                "I would start by defining the API contract, then check scalability risks around database access, caching, and rate limits.",
                "First, I would define the API contract. Then I would evaluate scalability risks, especially database access patterns, caching strategy, and rate limits.",
                ["API contract", "rate limits", "trade-off"],
                "You sound more senior when you explain why each decision matters, not only what you would do.",
                "How would you monitor this API after release?"),
            PracticeModes.Converter => new PracticeFeedback(
                "Here is a more natural professional version of the technical explanation.",
                "The service processes requests asynchronously and publishes messages to a queue for other services to consume.",
                "This service handles requests asynchronously by publishing messages to a queue, allowing downstream services to process them independently.",
                ["asynchronously", "publish messages", "downstream services"],
                "The technical idea is clear. Use verbs like process, publish, consume, and handle for professional engineering English.",
                "Which downstream service consumes the message from the queue?"),
            _ => new PracticeFeedback(
                "That makes sense. You found a problem in the payment API and want to explain the root cause clearly.",
                "I fixed a bug in the payment API, but I am not sure how to explain the root cause clearly.",
                "I investigated the payment API issue and found that the root cause was unclear error handling during failed transactions.",
                ["root cause", "investigate", "error handling"],
                "Your meaning is understandable. Use specific nouns like issue, root cause, and transaction to sound more precise.",
                "What evidence helped you identify the root cause of the API issue?")
        };
    }
}
