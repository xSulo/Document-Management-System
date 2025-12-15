namespace dms.Api.Messaging
{
    public sealed class GenAiResultMessage
    {
        public long DocumentId { get; set; }
        public string? Summary { get; set; }
        public string? Model { get; set; }
        public int ProcessingTimeMs { get; set; }
    } 
}