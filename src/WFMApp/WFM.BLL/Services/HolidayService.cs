using System;
using Nager.Date;
using System.Threading.Tasks;

using WFM.BLL.Interfaces;

namespace WFM.BLL.Services
{
    public class HolidayService : IHolidayService
    {
        public Task<int> DaysTimeOff(DateTime startDate, DateTime endDate, CountryCode countryCode)
        {
            int timeOffDays = (endDate - startDate).Days + 1;

            //Check if endDate is before startDate
            if (timeOffDays < 0)
            {
                throw new InvalidOperationException();
            }

            for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
            {
                if (DateSystem.IsWeekend(day, countryCode) || DateSystem.IsPublicHoliday(day, countryCode))
                {
                    timeOffDays--;
                }
            }

            return Task.FromResult(timeOffDays);
        }
    }
}
