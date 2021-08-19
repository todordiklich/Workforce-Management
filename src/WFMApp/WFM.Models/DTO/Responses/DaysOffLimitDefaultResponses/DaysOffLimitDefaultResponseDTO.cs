using System.ComponentModel.DataAnnotations;
using WFM.Common;

namespace WFM.Models.DTO.Responses.DaysOffLimitDefaultResponses
{
    public class DaysOffLimitDefaultResponseDTO
    {
        public string CountryCode { get; set; }

        public int PaidDaysOff { get; set; }

        public int UnpaidDaysOff { get; set; }

        public int SickLeaveDaysOff { get; set; }
    }
}
