using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using WFM.Common;

namespace WFM.DAL.Entities
{
    public class DaysOffLimitDefault
    {
        public DaysOffLimitDefault()
        {
            Users = new List<User>();
        }

        [Key]
        [MaxLength(GlobalConstants.CountryCodeLength)]
        public string CountryCode { get; set; }

        public List<User> Users { get; set; }

        public int PaidDaysOff { get; set; }

        public int UnpaidDaysOff { get; set; }

        public int SickLeaveDaysOff { get; set; }
    }
}
