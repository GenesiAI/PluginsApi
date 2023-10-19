using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiPlugin.Domain.Common;

namespace AiPlugin.Domain.Plugin
{
    [Table("PluginWhitelistedUser")]
    public class PluginWhitelistedUser : IDeleted
    {
        [Key]
        [MaxLength(50)]
        public string Email { get; set; } = null!;
        public bool isDeleted { get; set; }
        public virtual IEnumerable<PluginWhitelist> PluginWhitelists { get; set; } = null!;
    }
}
