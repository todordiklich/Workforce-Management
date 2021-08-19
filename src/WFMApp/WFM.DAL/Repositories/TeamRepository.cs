using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using WFM.DAL.Context;
using WFM.DAL.Entities;
using WFM.DAL.Repositories.Interfaces;
using WFM.DAL.Repositories.BaseGenericRepository;

namespace WFM.DAL.Repositories
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        public TeamRepository(AppEntityContext dbContext)
        : base(dbContext)
        {
            
        }

        public async Task<Team> AddTeamAsync(Team team)
        {
            return await AddAsync(team);
        }

        public async Task<Team> UpdateTeamAsync(Team team, Guid currentUserId)
        {
            return await UpdateAsync(team);
        }

        public async Task<Team> DeleteTeamAsync(Team team)
        {
            return await DeleteAsync(team);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await GetAll().Include(x => x.TeamMembers).ToListAsync();
        }

        public async Task<List<Team>> GetAllWithDeletedAsync()
        {
            return await GetAll().ToListAsync();
        }

        public async Task<Team> GetByIdAsync(Guid id)
        {
            return await GetAll().Include(x => x.TeamMembers).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team> GetByNameAsync(string name)
        {
            return await GetAll().FirstOrDefaultAsync(t => t.TeamName == name);
        }

        public async Task<Team> AddTeamMember(Team team)
        {         
            return await UpdateAsync(team);
        }

        public async Task<Team> GetByIdWithMembers(Guid teamId, Guid userId)
        {
            var teamWithMember = await GetAll().Where(x => x.Id == teamId).Include(x => x.TeamMembers.Where(x => x.Id == userId)).FirstOrDefaultAsync();

            return teamWithMember;
        }
    }
}
