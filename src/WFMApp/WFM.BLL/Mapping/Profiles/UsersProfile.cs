using AutoMapper;

using WFM.DAL.Entities;
using WFM.Models.Requests.UserRequests;
using WFM.Models.DTO.Responses.UserResponses;
using WFM.Models.DTO.Responses;

namespace WFM.BLL.Mapping.Profiles
{
    public class UsersProfile : Profile
    {
        public UsersProfile()
        {
            CreateMap<User, UserResponseDTO>();
            CreateMap<UserCreateRequestDTO, User>();
            CreateMap<UserEditRequestDTO, User>();
            CreateMap<User, CheckRemainingDaysResponseDTO>();
        }
    }
}
