using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface IApprovalRepository
    {
        Task<Approval> AddApprovalAsync(Approval approval);

        Task<Approval> UpdateApprovalAsync(Approval approval);

        Task<Approval> DeleteApprovalAsync(Approval approval);

        Task<List<Approval>> GetAllAsync();

        Task<List<Approval>> GetAllRequestApprovalsAsync(Guid requestId);

        Task<Approval> FindByUserIdAndRequestId(Guid userId, Guid requestId);
    }
}
