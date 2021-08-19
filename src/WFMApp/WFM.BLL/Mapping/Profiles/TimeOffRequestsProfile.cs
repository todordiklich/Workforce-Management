using AutoMapper;

using WFM.DAL.Entities;
using WFM.Models.DTO.Requests.TimeOffRequestRequests;
using WFM.Models.DTO.Responses.TimeOffRequestResponses;
using WFM.Models.DTO.Responses.UserResponses;

namespace WFM.BLL.Mapping.Profiles
{
    public class TimeOffRequestsProfile : Profile
    {
        public TimeOffRequestsProfile()
        {
            CreateMap<TimeOffRequest, TimeOffRequestResponseDTO>()
            .ForMember(destination => destination.UserId,
                map => map.MapFrom(
                    source => source.User.Id))
            .ForMember(destination => destination.UserName,
                map => map.MapFrom(
                    source => source.User.UserName));
            CreateMap<Approval, ApprovalsResponseDTO>()
                .ForMember(destination => destination.TeamLeaderUserName,
                    map => map.MapFrom(
                        source => source.TeamLeader.UserName));

            CreateMap<TimeOffRequestCreateDTO, TimeOffRequest>();
            CreateMap<TimeOffRequestEditDTO, TimeOffRequest>();
            CreateMap<TimeOffRequestApproveDTO, TimeOffRequest>();
            CreateMap<TimeOffRequestApproveDTO, Approval>();
            CreateMap<User, CheckRemainingDaysOffResponseDTO>();
        }
    }
}
