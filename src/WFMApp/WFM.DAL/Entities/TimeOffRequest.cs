using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using WFM.Common;
using WFM.DAL.Entities.Common;
using WFM.DAL.Enums;

namespace WFM.DAL.Entities
{
    public class TimeOffRequest : AuditableEntity
    {
        public User User { get; set; }

        public TimeOffReason RequestType { get; set; }

        [MaxLength(GlobalConstants.DescriptionLength)]
        public string Description { get; set; }

        public TimeOffStatus RequestStatus { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int RequestedDays { get; set; }

        public ICollection<Approval> Approvals { get; set; }
    }
}
