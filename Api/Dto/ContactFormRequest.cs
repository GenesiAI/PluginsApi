using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto
{
    public class ContactFormRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Message { get; set; } = null!;
    }
}