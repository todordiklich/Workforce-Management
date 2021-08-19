using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM.Common;

namespace WFM.Models.DTO.Requests.DaysOffLimitDefaultRequests
{
    public class DaysOffLimitDefaultEditRequeseDTO
    {
        [Required]
        [MaxLength(GlobalConstants.CountryCodeLength)]
        public string CountryCode { get; set; }

        [Required]
        public int PaidDaysOff { get; set; }

        [Required]
        public int UnpaidDaysOff { get; set; }

        [Required]
        public int SickLeaveDaysOff { get; set; }
    }
}
