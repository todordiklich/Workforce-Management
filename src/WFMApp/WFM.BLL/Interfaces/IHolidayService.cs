using System;
using Nager.Date;
using System.Threading.Tasks;

namespace WFM.BLL.Interfaces
{
    public interface IHolidayService
    {
        public Task<int> DaysTimeOff(DateTime startDate, DateTime endDate, CountryCode countryCode);
    }
}
