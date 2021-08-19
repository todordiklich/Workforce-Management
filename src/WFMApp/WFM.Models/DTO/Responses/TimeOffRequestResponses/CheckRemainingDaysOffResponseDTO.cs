using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFM.Models.DTO.Responses.UserResponses
{
    public class CheckRemainingDaysOffResponseDTO
    {
        public string UserName { get; set; }

        public int AvailablePaidDaysOff { get; set; }

        public int AvailableUnpaidDaysOff { get; set; }

        public int AvailableSickLeaveDaysOff { get; set; }
    }
}
