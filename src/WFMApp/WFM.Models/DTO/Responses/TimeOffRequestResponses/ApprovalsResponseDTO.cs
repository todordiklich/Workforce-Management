using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM.DAL.Enums;

namespace WFM.Models.DTO.Responses.TimeOffRequestResponses
{
    public class ApprovalsResponseDTO
    {
        public Guid TimeOffRequestId { get; set; }

        public string TeamLeaderUserName { get; set; }

        public Guid TeamLeaderId { get; set; }

        public TimeOffApprovalStatus ApprovalStatus { get; set; }
    }
}
