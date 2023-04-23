using System.Text.Json.Serialization;

namespace AiPlugin.Domain.Manifest
{
    public class AiPluginManifest
    {
        [JsonPropertyName("schema_version")]
        public string SchemaVersion { get; set; } = null!;

        [JsonPropertyName("name_for_human")]
        public string NameForHuman { get; set; } = null!;

        [JsonPropertyName("name_for_model")]
        public string NameForModel { get; set; } = null!;

        [JsonPropertyName("description_for_human")]
        public string DescriptionForHuman { get; set; } = null!;

        [JsonPropertyName("description_for_model")]
        public string DescriptionForModel { get; set; } = null!;

        [JsonPropertyName("auth")]
        public Auth Auth { get; set; } = null!;

        [JsonPropertyName("api")]
        public Api Api { get; set; } = null!;

        [JsonPropertyName("logo_url")]
        public string LogoUrl { get; set; } = null!;

        [JsonPropertyName("contact_email")]
        public string ContactEmail { get; set; } = null!;

        [JsonPropertyName("legal_info_url")]
        public string LegalInfoUrl { get; set; } = null!;
    }
}