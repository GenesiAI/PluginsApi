﻿using AiPlugin.Domain.User;
using AiPlugin.Infrastructure;

namespace AiPlugin.Application.Users
{

    public class UserRepository
    {
        private readonly AiPluginDbContext dbContext;

        public UserRepository(AiPluginDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Returns the User object
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User Object</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<User> GetUser(string id)
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user is null || user.isDeleted)
            {
                throw new KeyNotFoundException($"User with id {id} was not found");
            }
            return user;
        }

        /// <summary>
        /// Adds a new user to the system
        /// </summary>
        /// <param name="user">Object that describes the new User</param>
        /// <returns>Task object</returns>
        public async Task<User> AddNewUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Update the user informations
        /// </summary>
        /// <param name="user">Object that describes the User</param>
        /// <returns>Task object</returns>
        public async Task UpdateUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return;
        }

        /// <summary>
        /// Deletes the user defined by the id passed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        private async Task DeleteUser(string id, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id);
            User user = await dbContext.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found");
            }
            user.isDeleted = true;
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

    }
}
