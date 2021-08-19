using System;

using WFM.DAL.Enums;
using WFM.DAL.Entities.Common;

namespace WFM.DAL.Entities
{
    public class Approval : IAuditableEntity, IDeletableEntity
    {
        public Guid TimeOffRequestId { get; set; }

        public TimeOffRequest TimeOffRequest { get; set; }

        public Guid TeamLeaderId { get; set; }

        public User TeamLeader { get; set; }

        public TimeOffApprovalStatus ApprovalStatus { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime DeletedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
    