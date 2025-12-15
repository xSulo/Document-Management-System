namespace OcrWorker.Contracts;

public sealed record OcrRequest(
    long DocumentId,
    string Title,
    string FilePath,
    DateTimeOffset UploadedAtUtc
);
