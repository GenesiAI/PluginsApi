namespace AiPlugin.Api.Dto
{
    public class SectionCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

}