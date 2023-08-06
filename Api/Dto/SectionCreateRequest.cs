using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto
{
    public class SectionCreateRequest
    {
        [MaxLength(200)] //defined by us
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        [MaxLength(100000)]
        public string Content { get; set; } = null!;
    }

}