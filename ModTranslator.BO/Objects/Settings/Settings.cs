namespace ModTranslator.BO.Objects.Settings
{
    public class APISettings
    {
        public string Url { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class RequestsSettings
    {
        public int MaxConcurrentRequests { get; set; } = 1;
        public int TimeoutSeconds { get; set; } = 60;
        public int MaxLengthOfRequests { get; set; } = 100;
    }

    public class AppSettings
    {
        public APISettings APISettings { get; set; } = new();

        public RequestsSettings RequestsSettings { get; set; } = new();
    }
}
