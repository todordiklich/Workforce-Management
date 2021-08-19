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
    public class ApprovalRepository : Repository<Approval>, IApprovalRepository
    {
        public ApprovalRepository(AppEntityContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Approval>> GetAllAsync()
        {
            return await GetAll()
                .Include(t => t.TimeOffRequest)
                .ThenInclude(r => r.User)
                .Include(t => t.TeamLeader)
                .ToListAsync();
        }

        public async Task<List<Approval>> GetAllRequestApprovalsAsync(Guid requestId)
        {
            return await GetAll()
                .Where(a => a.TimeOffRequestId == requestId)
                .Include(t => t.TimeOffRequest)
                .ThenInclude(r => r.User)
                .Include(t => t.TeamLeader)
                .ToListAsync();
        }

        public async Task<Approval> FindByUserIdAndRequestId(Guid userId, Guid requestId)
        {
            return await GetAll()
                .Where(a => a.TeamLeaderId == userId && a.TimeOffRequestId == requestId)
                .Include(t => t.TimeOffRequest)
                .Include(t => t.TeamLeader)
                .FirstOrDefaultAsync();
        }

        public async Task<Approval> AddApprovalAsync(Approval approval)
        {
            return await AddAsync(approval);
        }

        public async Task<Approval> UpdateApprovalAsync(Approval approval)
        {
            return await UpdateAsync(approval);
        }

        public async Task<Approval> DeleteApprovalAsync(Approval approval)
        {
            return await DeleteAsync(approval);
        }
    }
}
