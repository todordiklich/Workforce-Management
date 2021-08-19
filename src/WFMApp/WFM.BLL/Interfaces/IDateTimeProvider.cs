using System;

namespace WFM.BLL.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        int CurrentMonth { get; }
    }
}
