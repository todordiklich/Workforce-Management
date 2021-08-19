using AutoMapper;

using WFM.DAL.Entities;
using WFM.Models.Requests.TeamRequests;
using WFM.Models.DTO.Responses.TeamResponses;
using WFM.Models.DTO.Responses.UserResponses;

namespace WFM.BLL.Mapping.Profiles
{
    public class TeamsProfile : Profile
    {
        public TeamsProfile()
        {
            CreateMap<TeamBaseRequestDTO, Team>();
            CreateMap<Team, TeamResponseDTO>();
            CreateMap<User, UserDetailsResponseDTO>();
        }
    }
}
