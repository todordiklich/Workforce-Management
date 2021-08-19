using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WFM.Common;
using WFM.DAL.Entities;
using WFM.DAL.Enums;

namespace WFM.Models.DTO.Responses.TimeOffRequestResponses
{
    public class TimeOffRequestResponseDTO : BaseResponseDTO
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public TimeOffReason RequestType { get; set; }

        [MaxLength(GlobalConstants.DescriptionLength)]
        public string Description { get; set; }

        public TimeOffStatus RequestStatus { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int RequestedDays { get; set; }
    }
}
