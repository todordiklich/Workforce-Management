using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.BLL.Interfaces
{
    public interface IDaysOffLimitDefaultService
    {
        Task<DaysOffLimitDefault> CreateAsync(DaysOffLimitDefault daysOffLimitDefault);
        Task<DaysOffLimitDefault> DeleteAsync(DaysOffLimitDefault daysOffLimitDefault);
        Task<ICollection<DaysOffLimitDefault>> GetAllAsync();
        Task<DaysOffLimitDefault> UpdateAsync(DaysOffLimitDefault daysOffLimitDefault);
        Task<DaysOffLimitDefault> FindByCountryCode(string countryCode);
    }
}