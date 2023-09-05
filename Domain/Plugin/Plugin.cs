using AiPlugin.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Domain.Plugin
{
    // a plugin is owned by a user, plugins are made of sections
    public class Plugin : EntityBase, IDeleted
    {
        public bool IsActive { get; set; }
        public string UserId { get; set; } = null!;
        public virtual IEnumerable<Section>? Sections { get; set; } = null!;

        [MaxLength(20)]
        public string NameForHuman { get; set; } = null!;

        [MaxLength(50)]
        public string NameForModel { get; set; } = null!;

        [MaxLength(100)]
        public string DescriptionForHuman { get; set; } = null!;

        [MaxLength(8000)]
        public string DescriptionForModel { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string LegalInfoUrl { get; set; } = null!;
        public DateTime CreationDateTime { get; set; } = DateTime.UtcNow;
        public bool isDeleted { get; set; }
    }
}