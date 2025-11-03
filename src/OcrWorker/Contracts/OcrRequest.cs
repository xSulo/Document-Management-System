namespace OcrWorker.Contracts;

public sealed record OcrRequest(
    Guid DocumentId,
    string Bucket,
    string ObjectKey,
    string? Language // "eng", "deu", ...
);
