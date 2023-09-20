using AiPlugin.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiPlugin.Domain.User
{
    /// <summary>
    /// User entity.
    /// Each user has an Id, email, firebaseId and datetime of creation
    /// </summary>
    public class User : EntityBase, IDeleted
    {
        public string UserId { get; set; }
        public string Email {  get; set; }
        public int FirebaseId { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool isDeleted { get; set; }
    }
}
