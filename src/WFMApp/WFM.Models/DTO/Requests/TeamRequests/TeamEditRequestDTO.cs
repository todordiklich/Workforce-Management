using System;
using System.ComponentModel.DataAnnotations;

namespace WFM.Models.Requests.TeamRequests
{
    public class TeamEditRequestDTO : TeamBaseRequestDTO
    {
        [Required]
        public Guid Id { get; set; }
    }
}
