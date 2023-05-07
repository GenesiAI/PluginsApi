namespace AiPlugin.Application.Old.OpenAi.Models;

public class Choice
{
    public string text { get; set; } = null!;
    public int index { get; set; }
    public object? logprobs { get; set; }
    public string finish_reason { get; set; } = null!;
}
