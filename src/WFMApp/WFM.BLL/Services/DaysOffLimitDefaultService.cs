using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories.Interfaces;

namespace WFM.BLL.Services
{
    public class DaysOffLimitDefaultService : IDaysOffLimitDefaultService
    {
        private readonly IDaysOffLimitDefaultRepository _daysOffLimitDefaultRepository;

        public DaysOffLimitDefaultService(IDaysOffLimitDefaultRepository daysOffLimitDefaultRepository)
        {
            _daysOffLimitDefaultRepository = daysOffLimitDefaultRepository;
        }

        public async Task<ICollection<DaysOffLimitDefault>> GetAllAsync()
        {
            return await _daysOffLimitDefaultRepository.GetAllAsync();
        }

        public async Task<DaysOffLimitDefault> CreateAsync(DaysOffLimitDefault daysOffLimitDefault)
        {
            return await _daysOffLimitDefaultRepository.CreateDaysOffLimitDefaultAsync(daysOffLimitDefault);
        }

        public async Task<DaysOffLimitDefault> UpdateAsync(DaysOffLimitDefault daysOffLimitDefault)
        {
            return await _daysOffLimitDefaultRepository.UpdateDaysOffLimitDefaultAsync(daysOffLimitDefault);
        }

        public async Task<DaysOffLimitDefault> DeleteAsync(DaysOffLimitDefault daysOffLimitDefault)
        {
            return await _daysOffLimitDefaultRepository.DeleteDaysOffLimitDefaultAsync(daysOffLimitDefault);
        }
        public async Task<DaysOffLimitDefault> FindByCountryCode(string countryCode)
        {
            return await _daysOffLimitDefaultRepository.FindByCountryCodeAsync(countryCode);
        }
    }
}
