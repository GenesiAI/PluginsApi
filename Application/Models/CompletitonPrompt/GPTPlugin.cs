namespace AiPlugin.Application.models.CompletitonPrompt;

public class GPTPlugin
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<GPTAPI> Apis { get; set; } = null!;
}