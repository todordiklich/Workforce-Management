using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WFM.DAL.Context;
using WFM.DAL.Entities;
using WFM.DAL.Repositories.Interfaces;

namespace WFM.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Create(User user, string password)
        {
            var identityResult = await _userManager.CreateAsync(user, password);
            return identityResult.Succeeded;
        }

        public async Task<bool> Update(User user)
        {
            var identityResult = await _userManager.UpdateAsync(user);
            return identityResult.Succeeded;
        }

        public async Task<bool> Delete(User user)
        {
            var identityResult = await _userManager.UpdateAsync(user);
            return identityResult.Succeeded;
        }

        public async Task<List<User>> GetAll()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<List<User>> GetAllWithDeleted()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<List<User>> GetUserTeamLeaders(Guid userId)
        {
            List<Guid> teamLeaderIds = await _userManager.Users
                .Where(u => u.Id == userId)
                .SelectMany(x => x.Teams.Where(t => t.TeamLeaderId != userId).Distinct()
                    .Select(t => t.TeamLeaderId)).ToListAsync();

            List<User> teamLeaders = new List<User>();

            foreach (var id in teamLeaderIds)
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                teamLeaders.Add(user);
            }

            return teamLeaders;
        }

        public async Task<User> FindById(Guid id)
        {
            return await _userManager.Users
                .Include(u => u.Teams)
                .Include(u => u.TimeOffRequests)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> FindByName(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<User> FindByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserInRole(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> CheckPassword(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> AddUserToRole(User user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);

            return result.Succeeded;
        }
    }
}
