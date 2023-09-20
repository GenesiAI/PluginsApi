using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto.User
{
    public class UserCreateRequest
    {
        [StringLength(50)]
        public string email { get; set; }
        public int FirebaseId { get; set; }
    }
}
