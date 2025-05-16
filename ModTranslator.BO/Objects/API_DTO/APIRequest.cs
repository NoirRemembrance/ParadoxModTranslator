using System.Text.Json.Serialization;

namespace ModTranslator.BO.Objects.API_DTO
{
    public class APIRequest
    {
        public class Data()
        {
            [JsonPropertyName("model")]
            public required string Model { get; set; }

            [JsonPropertyName("messages")]
            public required Message[] Messages { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
        }

        public class Message()
        {
            [JsonPropertyName("role")]
            public required string Role { get; set; }

            [JsonPropertyName("content")]
            public required string Content { get; set; }
        }

    }
}
