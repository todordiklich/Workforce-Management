using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.BLL.Interfaces
{
    public interface ITimeOffRequestService
    {
        Task<List<TimeOffRequest>> GetAllMyRequestsAsync(Guid userId);

        Task<List<TimeOffRequest>> GetAllMyAssignedRequestsAsync(Guid userId); 

        Task<TimeOffRequest> FindByIdAsync(Guid requestId);

        Task<TimeOffRequest> CreateAsync(TimeOffRequest timeOffRequest, Guid userId);

        Task<TimeOffRequest> UpdateAsync(TimeOffRequest timeOffRequest, Guid userId);

        Task<TimeOffRequest> DeleteAsync(TimeOffRequest timeOffRequest, Guid userId);

        Task<bool> CheckIfUserIsOwner(Guid timeOffRequestId, User user);

        Task<int> CancelUserTimeOffRequests(Guid userId);
    }
}
