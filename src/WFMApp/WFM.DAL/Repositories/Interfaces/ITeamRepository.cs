using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team> AddTeamAsync(Team team);

        Task<Team> UpdateTeamAsync(Team team, Guid currentUserId);

        Task<Team> DeleteTeamAsync(Team team);

        Task<List<Team>> GetAllAsync();

        Task<List<Team>> GetAllWithDeletedAsync();

        Task<Team> GetByIdAsync(Guid id);

        Task<Team> GetByNameAsync(string name);

        Task<Team> GetByIdWithMembers(Guid teamId, Guid userId);

        Task<Team> AddTeamMember(Team team);
    }
}
