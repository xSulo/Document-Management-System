namespace GenAIWorker.Contracts;

public sealed record GenAIRequest(long DocumentId, string Text);
