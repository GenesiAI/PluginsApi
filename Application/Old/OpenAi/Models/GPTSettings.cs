namespace AiPlugin.Application.Old.OpenAi.Models;

public class GPTSettings
{
    // public string model { get; set; } = null!;
    public double temperature { get; set; }
    public int max_tokens { get; set; }
    public int n { get; set; }
    public string ApiKey { get; set; } = null!;
}