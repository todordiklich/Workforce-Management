using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using WFM.DAL.Enums;
using WFM.DAL.Context;
using WFM.DAL.Entities;
using WFM.DAL.Repositories.Interfaces;
using WFM.DAL.Repositories.BaseGenericRepository;

namespace WFM.DAL.Repositories
{
    public class TimeOffRequestRepository : Repository<TimeOffRequest>, ITimeOffRequestRepository
    {
        public TimeOffRequestRepository(AppEntityContext dbContext)
        :base(dbContext)
        {
        }

        public async Task<TimeOffRequest> AddTimeOffRequestAsync(TimeOffRequest timeOffRequest)
        {
            return await AddAsync(timeOffRequest);
        }

        public async Task<TimeOffRequest> UpdateTimeOffRequestAsync(TimeOffRequest timeOffRequest)
        {
            return await UpdateAsync(timeOffRequest);
        }

        public async Task<TimeOffRequest> DeleteTimeOffRequestAsync(TimeOffRequest timeOffRequest)
        {
            return await DeleteAsync(timeOffRequest);
        }

        public async Task<List<TimeOffRequest>> GetAllMyRequestsAsync(Guid userId)
        {
            return await GetAll()
                .Include(t => t.User)
                .Where(r => r.User.Id == userId)
                .ToListAsync();
        }

        public async Task<List<TimeOffRequest>> GetAllMyAssignedRequestsAsync(Guid userId)
        {
            return await GetAll()
                .Include(t => t.User)
                .Where(r => r.RequestStatus != TimeOffStatus.Rejected && r.RequestStatus != TimeOffStatus.Canceled && r.Approvals.Any(a => a.TeamLeaderId == userId))
                .ToListAsync();
        }

        public async Task<List<TimeOffRequest>> GetAllAsync()
        {
            return await GetAll()
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<TimeOffRequest> FindByIdAsync(Guid id)
        {
            return await GetAll()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> CheckIfUserIsOwner(Guid timeOffRequestId, User user)
        {
            return await GetAll()
                .Include(t => t.User)
                .AnyAsync(t => t.Id == timeOffRequestId && t.User.Id == user.Id);
        }
    }
}
