namespace dms.Api.Messaging;

public record OcrJobMessage(long DocumentId, string Title, string FilePath, DateTimeOffset UploadedAtUtc);
