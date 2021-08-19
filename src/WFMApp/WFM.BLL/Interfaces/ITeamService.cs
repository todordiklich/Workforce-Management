using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.Models.Requests.TeamRequests;

namespace WFM.BLL.Interfaces
{
    public interface ITeamService
    {
        Task<List<Team>> GetAllAsync();

        Task<Team> CreateAsync(Team team, Guid creatorUserId);

        Task<Team> DeleteAsync(Team team, Guid currentUserId);

        Task<Team> UpdateAsync(Team team, Guid currentUserId);

        Task<Team> AddMemberAsync(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO, Guid currentUserId);

        Task<Team> RemoveMemberAsync(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO, Guid currentUserId);

        Task<Team> GetByNameAsync(string name);

        Task<Team> GetByIdAsync(Guid id);
    }
}
