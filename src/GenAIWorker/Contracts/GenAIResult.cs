using System;

namespace GenAIWorker.Contracts;

public sealed record GenAIResult
(
    long DocumentId,
    string Summary,
    string? Model,
    int ProcessingTimeMs
);
