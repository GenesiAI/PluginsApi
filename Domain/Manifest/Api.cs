using System.Text.Json.Serialization;

namespace AiPlugin.Domain.Manifest
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Api
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        [JsonPropertyName("is_user_authenticated")]
        public bool IsUserAuthenticated { get; set; }
    }


}