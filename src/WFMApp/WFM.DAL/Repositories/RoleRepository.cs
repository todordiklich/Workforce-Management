using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using WFM.DAL.Repositories.Interfaces;

namespace WFM.DAL.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> Create(IdentityRole role)
        {
            var identityResult = await _roleManager.CreateAsync(role);
            return identityResult.Succeeded;
        }

        public async Task<IdentityRole> FindByName(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }
    }
}
