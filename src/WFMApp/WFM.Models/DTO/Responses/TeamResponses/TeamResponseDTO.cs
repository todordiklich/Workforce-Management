using System;
using System.Collections.Generic;

using WFM.Models.DTO.Responses.UserResponses;

namespace WFM.Models.DTO.Responses.TeamResponses
{
    public class TeamResponseDTO : BaseResponseDTO
    {
        public string TeamName { get; set; }

        public string Description { get; set; }

        public Guid TeamLeaderId { get; set; }

        public ICollection<UserDetailsResponseDTO> TeamMembers { get; set; }
    }
}
