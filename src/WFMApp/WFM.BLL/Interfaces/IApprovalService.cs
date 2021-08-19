using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.BLL.Interfaces
{
    public interface IApprovalService
    {
        Task<List<Approval>> GetAllAsync();

        Task<List<Approval>> GetAllRequestApprovalsAsync(Guid requestId);

        Task<Approval> FindByUserIdAndRequestId(Guid userId, Guid requestId);

        Task<Approval> CreateAsync(Approval approval);

        Task<Approval> ApproveAsync(Approval approval);

        Task<Approval> RejectAsync(Approval approval);

    }
}
