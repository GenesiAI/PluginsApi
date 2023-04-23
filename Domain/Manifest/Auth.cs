using System.Text.Json.Serialization;

namespace AiPlugin.Domain.Manifest
{
    public class Auth
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;
    }


}