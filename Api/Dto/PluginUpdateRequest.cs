namespace AiPlugin.Api.Dto
{
    public class PluginUpdateRequest
    {
        public string NameForHuman { get; set; } = null!;
        public string NameForModel { get; set; } = null!;
        public string DescriptionForHuman { get; set; } = null!;
        public string DescriptionForModel { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string LegalInfoUrl { get; set; } = null!;
        public IEnumerable<SectionCreateRequest>? Sections { get; set; }
    }

}