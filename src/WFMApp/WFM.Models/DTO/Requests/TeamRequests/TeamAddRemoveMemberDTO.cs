using System;
using System.ComponentModel.DataAnnotations;

namespace WFM.Models.Requests.TeamRequests
{
    public class TeamAddRemoveMemberDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid TeamId { get; set; }
    }
}
