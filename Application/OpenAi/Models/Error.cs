namespace AiPlugin.Application.OpenAi.Models;

public class Error
{
    public string message { get; set; }= null!;
    public string type { get; set; }= null!;
    public object param { get; set; }= null!;
    public object code { get; set; }= null!;
}
