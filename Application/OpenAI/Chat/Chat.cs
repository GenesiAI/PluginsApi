using AiPlugin.Domain.Common;
using AiPlugin.Domain.Plugin;

namespace AiPlugin.Domain.Chat;
public class Chat /*: EntityBase*/
{
    public Plugin.Plugin AiPlugin { get; set; } = null!;
    public IEnumerable<Message> Messages { get; set; } = null!;
}
