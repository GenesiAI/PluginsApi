using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto.User
{
    public class UserCreateRequest
    {
        [StringLength(50)]
        public string Email { get; set; } = null!;
    }
}
