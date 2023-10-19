using AiPlugin.Domain.Plugin;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Application.Plugins
{
    public class PluginWhitelistRepository : IPluginWhitelistRepository
    {
        private readonly AiPluginDbContext dbContext;
        private readonly PluginRepository pluginRepository;
        private readonly PluginWhitelistedUserRepository userRepository;

        public PluginWhitelistRepository(AiPluginDbContext dbContext, PluginRepository pluginRepository, PluginWhitelistedUserRepository userRepository)
        {
            this.dbContext = dbContext;
            this.pluginRepository = pluginRepository;
            this.userRepository = userRepository;
        }

        public async Task<PluginWhitelist> Add(PluginWhitelist pluginWhitelist, string userId, CancellationToken cancellationToken = default)
        {

            if (pluginWhitelist == null)
            {
                return null;
            }
            Plugin plugin = null;
            PluginWhitelistedUser user = null;
            try
            {
                plugin = await pluginRepository.Get(pluginWhitelist.PluginId);
                user = await userRepository.Get(pluginWhitelist.Email);
            }
            catch (Exception ex)
            {
               
            }
            //if the plugin doesn't exist or the whitelisted user doesn't exist, add them
            if (user == null && pluginWhitelist?.PluginWhitelistedUser != null)
            {
                await userRepository.Add(pluginWhitelist.PluginWhitelistedUser, cancellationToken);
            }
            if (plugin == null && pluginWhitelist?.Plugin != null)
            {
                await pluginRepository.Add(pluginWhitelist.Plugin, userId);
            }

            dbContext.PluginWhitelists.Add(pluginWhitelist);
            await dbContext.SaveChangesAsync(cancellationToken);
            return pluginWhitelist;
        }

        public async Task Delete(Guid pluginId, string email, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.PluginWhitelists.FindAsync(pluginId, email);
            if (entity is null || entity.isDeleted)
                throw new KeyNotFoundException($"Whitelisted user with email {email} and pluging {pluginId} not found");
            entity.isDeleted = true;
            dbContext.PluginWhitelists.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<PluginWhitelist> Get(Guid pluginId, string email, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.PluginWhitelists
                .Include(x => x.PluginWhitelistedUser)
                .Include(x => x.Plugin)
                .SingleOrDefaultAsync(x => x.Email == email && x.Plugin.Id == pluginId, cancellationToken);
            if (entity is null || entity.isDeleted)
                throw new KeyNotFoundException($"Whiteliste user with email {email} and plugin {pluginId} not found");
            //enforce deletion
            entity.Plugin = entity.Plugin.isDeleted ? null : entity.Plugin;
            entity.PluginWhitelistedUser = entity.PluginWhitelistedUser.isDeleted ? null : entity.PluginWhitelistedUser;
            return entity;
        }

        public async Task<PluginWhitelist> Update(PluginWhitelist pluginWhitelist, CancellationToken cancellationToken = default)
        {
            await this.Get(pluginWhitelist.PluginId, pluginWhitelist.Email, cancellationToken); //used to throw exception if not found
            dbContext.PluginWhitelists.Update(pluginWhitelist);
            await dbContext.SaveChangesAsync(cancellationToken);
            return pluginWhitelist;
        }
    }
}
