using AutoMapper;

using WFM.DAL.Entities;
using WFM.Models.DTO.Requests.DaysOffLimitDefaultRequests;
using WFM.Models.DTO.Responses.DaysOffLimitDefaultResponses;

namespace WFM.BLL.Mapping.Profiles
{
    public class DaysOffLimitDefaultProfile : Profile
    {
        public DaysOffLimitDefaultProfile()
        {
            CreateMap<DaysOffLimitDefault, DaysOffLimitDefaultResponseDTO>();
            CreateMap<DaysOffLimitDefaultCreateRequeseDTO, DaysOffLimitDefault>();
            CreateMap<DaysOffLimitDefaultEditRequeseDTO, DaysOffLimitDefault>();
        }
    }
}
