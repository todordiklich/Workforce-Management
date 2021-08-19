using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<bool> Create(IdentityRole role);

        Task<IdentityRole> FindByName(string roleName);
    }
}
