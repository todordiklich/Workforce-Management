using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WFM.Common;
using WFM.DAL.Entities.Common;

namespace WFM.DAL.Entities
{
    public class Team : AuditableEntity
    {
        public Team()
        {
            TeamMembers = new HashSet<User>();
        }

        [MaxLength(GlobalConstants.MaxLength)]
        public string TeamName { get; set; }

        [MaxLength(GlobalConstants.DescriptionLength)]
        public string Description { get; set; }

        public Guid TeamLeaderId { get; set; }

        public ICollection<User> TeamMembers { get; set; }
    }
}
