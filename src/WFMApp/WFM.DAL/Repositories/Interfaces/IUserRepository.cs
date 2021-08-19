using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> Create(User user, string password);

        Task<bool> Update(User user);

        Task<bool> Delete(User user);

        Task<List<User>> GetAll();

        Task<List<User>> GetAllWithDeleted();

        Task<List<User>> GetUserTeamLeaders(Guid userId);

        Task<User> FindById(Guid id);

        Task<User> FindByName(string userName);

        Task<User> FindByEmail(string email);

        Task<bool> IsUserInRole(User user, string roleName);

        Task<bool> CheckPassword(User user, string password);

        public Task<bool> AddUserToRole(User user, string roleName);
    }
}
