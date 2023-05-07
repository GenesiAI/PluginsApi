namespace AiPlugin.Application.Old.OpenAi.Models;

public class CompletationResponse : ErrorResponse
{
    public string id { get; set; } = null!;
    public string @object { get; set; } = null!;
    public int created { get; set; }
    public string model { get; set; } = null!;
    public List<Choice> choices { get; set; } = null!;
    public Usage usage { get; set; } = null!;
}
