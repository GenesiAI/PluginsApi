using AiPlugin.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Domain.Plugin;

// each section has an id, name, description and a content
public class Section : EntityBase, IDeleted
{
    public Guid PluginId { get; set; }

    [MaxLength(50)] //defined by us
    public string Name { get; set; } = null!;

    [MaxLength(200)]
    public string Description { get; set; } = null!;

    [MaxLength(100000)]
    public string Content { get; set; } = null!;
    public bool isDeleted { get; set; }
}