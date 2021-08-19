using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFM.BLL.Interfaces;
using WFM.DAL.Context;
using WFM.DAL.Entities;

namespace WFM.BLL.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class RecalculateDaysOffJob : IJob
    {
        private readonly IDaysOffLimitDefaultService _daysOffLimitDefaultService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public RecalculateDaysOffJob(IDaysOffLimitDefaultService daysOffLimitDefaultService, IUserService userService, IEmailService emailService)
        {
            _daysOffLimitDefaultService = daysOffLimitDefaultService;
            _userService = userService;
            _emailService = emailService;
        }

        public async Task Execute(IJobExecutionContext jobExecutionContext)
        {
            User adminUser = await _userService.GetByNameAsync("admin");
            var adminUserGuid = adminUser.Id;

            var allUsersFromDB = await _userService.GetAllAsync();
            var allAvailableDefaultDaysOff = await _daysOffLimitDefaultService.GetAllAsync();

            foreach (var user in allUsersFromDB)
            {
                var userDefaultDaysOffPerCountryCode = await _daysOffLimitDefaultService.FindByCountryCode(user.CountryOfResidence);

                user.AvailablePaidDaysOff += userDefaultDaysOffPerCountryCode.PaidDaysOff;
                user.AvailableSickLeaveDaysOff = userDefaultDaysOffPerCountryCode.SickLeaveDaysOff;
                user.AvailableUnpaidDaysOff = userDefaultDaysOffPerCountryCode.UnpaidDaysOff;

                await _userService.UpdateAsync(user, adminUserGuid);
            }

            //send mail to the system that the task is completed
            var subject = $"The {nameof(RecalculateDaysOffJob)} was executed successfully!";
            var message = "All users DaysOffs have been updated!";
            var toName = adminUser.FirstName + " " + adminUser.LastName;
            var toEmailAddress = adminUser.Email;

            await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
        }
    }
}
