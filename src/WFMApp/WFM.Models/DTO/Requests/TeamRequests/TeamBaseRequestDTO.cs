using System;
using System.ComponentModel.DataAnnotations;
using WFM.Common;

namespace WFM.Models.Requests.TeamRequests
{
    public class TeamBaseRequestDTO
    {
        [Required]
        [MaxLength(GlobalConstants.MaxLength)]
        public string TeamName { get; set; }

        [Required]
        [MaxLength(GlobalConstants.DescriptionLength)]
        public string Description { get; set; }

        [Required]
        public Guid TeamLeaderId { get; set; }
    }
}
