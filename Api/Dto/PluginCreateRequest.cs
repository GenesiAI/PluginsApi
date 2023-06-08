using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto;
public class PluginCreateRequest
{
    [StringLength(20)]
    public string NameForHuman { get; set; } = null!;

    [StringLength(50)]
    public string NameForModel { get; set; } = null!;

    [StringLength(100)]
    public string DescriptionForHuman { get; set; } = null!;

    [StringLength(8000)]
    public string DescriptionForModel { get; set; } = null!;

    public string LogoUrl { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string LegalInfoUrl { get; set; } = null!;

    public IEnumerable<SectionCreateRequest>? Sections { get; set; }
}