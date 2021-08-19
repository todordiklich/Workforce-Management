using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.BLL.Interfaces
{
    public interface IUserService
    {
        public Task<List<User>> GetAllAsync();

        public Task<bool> CreateAsync(User user, string password, string roleName, Guid creatorId);

        public Task<bool> DeleteAsync(User user);

        public Task<User> GetByIdAsync(Guid Id);

        public Task<User> GetByNameAsync(string userName);

        public Task<bool> AddUserToRole(User user, string roleName);

        public Task UpdateAsync(User user, Guid modifierId);

        public Task<bool> CheckIfRoleExists(string roleName);

        public Task<bool> CheckIfCountryOfResidenceExists(string countryCode);

        public Task<bool> IsUserInRole(User user, string roleName);
    }
}
