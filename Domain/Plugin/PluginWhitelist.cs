using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiPlugin.Domain.Common;

namespace AiPlugin.Domain.Plugin
{
    public class PluginWhitelist : IDeleted
    {
        public PluginWhitelist(Guid pluginId, string email, Plugin plugin, PluginWhitelistedUser pluginWhitelistedUser, bool isDeleted)
        {
            PluginId = pluginId;
            Email = email;
            Plugin = plugin;
            PluginWhitelistedUser = pluginWhitelistedUser;
            this.isDeleted = isDeleted;
        }
        // EF constructor
        private PluginWhitelist(Guid pluginId, string email, bool isDeleted) : this(pluginId, email, null!, null!, isDeleted)
        {
        }
      
        public Guid PluginId { get; set; }
        [MaxLength(50)]
    
        public string Email { get; set; }
        public Plugin Plugin { get; set; } = null!;
        public PluginWhitelistedUser PluginWhitelistedUser { get; set; } = null!;
        public bool isDeleted { get; set; }
    }
}

