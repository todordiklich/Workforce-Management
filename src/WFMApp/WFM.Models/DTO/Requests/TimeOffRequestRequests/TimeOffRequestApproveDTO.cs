using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM.DAL.Enums;

namespace WFM.Models.DTO.Requests.TimeOffRequestRequests
{
    public class TimeOffRequestApproveDTO
    {
        [Required]
        public Guid TimeOffRequestId { get; set; }

        [Required]
        public TimeOffApprovalStatus ApprovalStatus { get; set; }
    }
}
