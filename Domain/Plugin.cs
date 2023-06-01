namespace AiPlugin.Domain
{
    //a plugin is owned by a user, plugins are made of sections
    public class Plugin : EntityBase, IDeleted
    {
        public string UserId { get; set; } = null!;
        public virtual IEnumerable<Section>? Sections { get; set; } = null!;
        // public string SchemaVersion { get; set; } = null!;
        public string NameForHuman { get; set; } = null!;
        public string NameForModel { get; set; } = null!;
        public string DescriptionForHuman { get; set; } = null!;
        public string DescriptionForModel { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string LegalInfoUrl { get; set; } = null!;
        public bool isDeleted { get; set; }
    }
}