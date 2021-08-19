using System;

using WFM.BLL.Interfaces;

namespace WFM.BLL.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public int CurrentMonth => DateTime.UtcNow.Month;
    }
}
