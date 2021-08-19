using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFM.Models.DTO.Responses
{
    public class CheckRemainingDaysResponseDTO
    {
        public string Username { get; set; }
        public int AvailablePaidDaysOff { get; set; }
        public int AvailableUnpaidDaysOff { get; set; }
        public int AvailableSickLeaveDaysOff { get; set; }
    }
}
