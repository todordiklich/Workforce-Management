using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

using WFM.Common;
using WFM.DAL.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFM.DAL.Entities
{
    public class User : IdentityUser<Guid> , IAuditableEntity
    {
        public User()
        {
            Teams = new HashSet<Team>();
            TimeOffRequests = new HashSet<TimeOffRequest>();
        }

        [MaxLength(GlobalConstants.MaxLength)]
        public string FirstName { get; set; }

        [MaxLength(GlobalConstants.MaxLength)]
        public string LastName { get; set; }

        [MaxLength(GlobalConstants.DaysOffLength)]
        public int AvailablePaidDaysOff { get; set; }

        [MaxLength(GlobalConstants.DaysOffLength)]
        public int AvailableUnpaidDaysOff { get; set; }

        [MaxLength(GlobalConstants.DaysOffLength)]
        public int AvailableSickLeaveDaysOff { get; set; }

        [ForeignKey("DaysOffLimitDefault")]
        public string CountryOfResidence { get; set; }

        public DaysOffLimitDefault DaysOffLimitDefault { get; set; }

        public ICollection<Team> Teams { get; set; }

        public ICollection<TimeOffRequest> TimeOffRequests { get; set; }

        public ICollection<Approval> Approvals { get; set; }

        //IAuditableEntity
        public Guid CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
        
        public bool IsDeleted { get; set; }

        public DateTime DeletedOn { get; set; }
    }
}
