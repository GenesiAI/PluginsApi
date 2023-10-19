using AiPlugin.Domain.Plugin;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Application.Plugins
{
    public class PluginWhitelistedUserRepository : IPluginWhitelistedUserRepository
    {
        private readonly AiPluginDbContext dbContext;

        public PluginWhitelistedUserRepository(AiPluginDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<PluginWhitelistedUser> Add(PluginWhitelistedUser pluginWhitelistedUser, CancellationToken cancellationToken = default)
        {
            dbContext.PluginWhitelistedUsers.Add(pluginWhitelistedUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            return pluginWhitelistedUser;
        }

        public async Task Delete(string email, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.PluginWhitelistedUsers.FindAsync(email);
            if (entity is null || entity.isDeleted)
                throw new KeyNotFoundException($"Whitelisted user with email {email} not found");
            entity.isDeleted = true;
            dbContext.PluginWhitelistedUsers.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<PluginWhitelistedUser> Get(string email, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.PluginWhitelistedUsers.Include(x => x.PluginWhitelists).SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
            if (entity is null || entity.isDeleted)
                throw new KeyNotFoundException($"Whitelisted user with email {email} not found");
            entity.PluginWhitelists = entity.PluginWhitelists?.Where(x => !x.isDeleted);
            return entity;
        }

        public async Task<PluginWhitelistedUser> Update(PluginWhitelistedUser pluginWhitelistedUser, CancellationToken cancellationToken = default)
        {
            await this.Get(pluginWhitelistedUser.Email, cancellationToken); //used to throw exception if not found
            dbContext.PluginWhitelistedUsers.Update(pluginWhitelistedUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            return pluginWhitelistedUser;
        }
    }

}
