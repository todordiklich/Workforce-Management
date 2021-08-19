using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;

namespace WFM.DAL.Repositories.Interfaces
{
    public interface IDaysOffLimitDefaultRepository
    {
        Task<DaysOffLimitDefault> CreateDaysOffLimitDefaultAsync(DaysOffLimitDefault daysOffLimitDefault);

        Task<DaysOffLimitDefault> DeleteDaysOffLimitDefaultAsync(DaysOffLimitDefault daysOffLimitDefault);

        Task<DaysOffLimitDefault> FindByCountryCodeAsync(string countryCode);

        Task<ICollection<DaysOffLimitDefault>> GetAllAsync();
        
        Task<DaysOffLimitDefault> UpdateDaysOffLimitDefaultAsync(DaysOffLimitDefault daysOffLimitDefault);
    }
}