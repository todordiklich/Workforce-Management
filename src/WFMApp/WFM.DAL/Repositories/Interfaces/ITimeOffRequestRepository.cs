using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface ITimeOffRequestRepository
    {
        Task<TimeOffRequest> AddTimeOffRequestAsync(TimeOffRequest timeOffRequest);

        Task<TimeOffRequest> UpdateTimeOffRequestAsync(TimeOffRequest timeOffRequest);

        Task<TimeOffRequest> DeleteTimeOffRequestAsync(TimeOffRequest timeOffRequest);

        Task<List<TimeOffRequest>> GetAllAsync();

        Task<List<TimeOffRequest>> GetAllMyRequestsAsync(Guid userId);

        Task<List<TimeOffRequest>> GetAllMyAssignedRequestsAsync(Guid userId);

        Task<TimeOffRequest> FindByIdAsync(Guid id);

        Task<bool> CheckIfUserIsOwner(Guid timeOffRequestId, User user);
    }
}
