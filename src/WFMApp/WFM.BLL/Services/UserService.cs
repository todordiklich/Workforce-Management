using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories.Interfaces;
using System.Linq;
using Quartz;
using WFM.BLL.Scheduler.Jobs;

namespace WFM.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly RoleManager<IdentityRole<Guid>> _roleMngr;
        private readonly IDaysOffLimitDefaultService _daysOffLimitDefaultService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITimeOffRequestService _timeOffRequestService;
        private readonly IEmailService _emailService;
        private readonly IScheduler _scheduler;

        public UserService(IUserRepository userRepository, RoleManager<IdentityRole<Guid>> roleMngr, IDaysOffLimitDefaultService daysOffLimitDefaultService, IDateTimeProvider dateTimeProvider, ITimeOffRequestService timeOffRequestService, IEmailService emailService, IScheduler scheduler)
        {
            _userRepository = userRepository;
            _roleMngr = roleMngr;
            _daysOffLimitDefaultService = daysOffLimitDefaultService;
            _dateTimeProvider = dateTimeProvider;
            _timeOffRequestService = timeOffRequestService;
            _emailService = emailService;
            _scheduler = scheduler;
        }

        public async Task<bool> CreateAsync(User user, string password, string roleName, Guid creatorId)
        {
            if (!await CheckIfRoleExists(roleName) || !await CheckIfCountryOfResidenceExists(user.CountryOfResidence))
            {
                return false;
            }

            if (await _userRepository.FindByName(user.UserName) != null)
            {
                return false;
            }

            if (await _userRepository.FindByEmail(user.Email) != null)
            {
                return false;
            }
            
            DaysOffLimitDefault daysOffDefaultLimits = await _daysOffLimitDefaultService.FindByCountryCode(user.CountryOfResidence);

            await CalculateDaysOffForNewUser(user, daysOffDefaultLimits);

            var dateTimeNow = _dateTimeProvider.UtcNow;
            user.CreatedOn = dateTimeNow;
            user.ModifiedOn = dateTimeNow;
            user.CreatedBy = creatorId;
            user.ModifiedBy = creatorId;

            return await _userRepository.Create(user, password);
        }

        public async Task<bool> DeleteAsync(User user)
        {
            #region send email notification that the user is removed from the team

            // send emails to all team members
            var admin = await _userRepository.FindByName("admin");

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(user.Id);
            List<User> userTeamMembers = user.Teams.SelectMany(t => t.TeamMembers).Where(u => u.Id != user.Id).Distinct().ToList();

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(admin);

            List<User> allEmailRecipientsWithMembers = new List<User>();
            allEmailRecipientsWithMembers.AddRange(allEmailRecipients);
            allEmailRecipientsWithMembers.AddRange(userTeamMembers);

            //send the emails

            // setup the content of the mail
            var subject = $"{user.FirstName} {user.LastName} - {user.UserName} has left our company!";
            var message = $"{user.FirstName} {user.LastName} is no longer a part of your teams and should not have access to any company projects or assets.";

            foreach (var member in allEmailRecipientsWithMembers)
            {
                var toName = member.FirstName + " " + member.LastName;
                var toEmailAddress = member.Email;

                //avoid no recipients
                if (member is not null)
                {
                    await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                }
            }

            #endregion

            //trigger the job and pass the userId and modifierId

            var deleteTimeOffRequestJobTrigger = DeleteTimeOffRequestJob.GetTrigger();
            var jobDetail = DeleteTimeOffRequestJob.CreateJob(user.Id, user.ModifiedBy);

            await _scheduler.ScheduleJob(jobDetail, deleteTimeOffRequestJobTrigger);

            //GDPR the user data
            int paidDaysOffToReturn = await _timeOffRequestService.CancelUserTimeOffRequests(user.Id);

            user.AvailablePaidDaysOff += paidDaysOffToReturn;
            user.IsDeleted = true;
            user.DeletedOn = _dateTimeProvider.UtcNow;
            user.FirstName = null;
            user.LastName = null;
            user.CountryOfResidence = null;
            user.UserName = user.Id.ToString();
            user.Email = null;

            return await _userRepository.Delete(user);
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _userRepository.FindById(id);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User> GetByNameAsync(string userName)
        {
            return await _userRepository.FindByName(userName);
        }

        public async Task<bool> CheckIfRoleExists(string roleName)
        {
            return await _roleMngr.RoleExistsAsync(roleName);
        }

        public async Task<bool> CheckIfCountryOfResidenceExists(string countryCode)
        {
            if (await _daysOffLimitDefaultService.FindByCountryCode(countryCode) == null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> AddUserToRole(User user, string roleName)
        {
            if (!await _roleMngr.RoleExistsAsync(roleName))
            {
                return false;
            }

            return await _userRepository.AddUserToRole(user, roleName);
        }

        public async Task UpdateAsync(User user, Guid modifier)
        {
            user.ModifiedOn = _dateTimeProvider.UtcNow;
            user.ModifiedBy = modifier;
            await _userRepository.Update(user);
        }

        public async Task<bool> IsUserInRole(User user, string roleName)
        {
            return await _userRepository.IsUserInRole(user, roleName);
        }

        private Task CalculateDaysOffForNewUser(User user, DaysOffLimitDefault daysOffDefaultLimits)
        {
            int currentMonth = _dateTimeProvider.CurrentMonth;
            double daysOffCoefficient = 12.0 / currentMonth;

            user.AvailablePaidDaysOff = daysOffDefaultLimits.PaidDaysOff + 1 - (int)(daysOffDefaultLimits.PaidDaysOff / daysOffCoefficient);
            user.AvailableUnpaidDaysOff = daysOffDefaultLimits.UnpaidDaysOff + 1 - (int)(daysOffDefaultLimits.UnpaidDaysOff / daysOffCoefficient);
            user.AvailableSickLeaveDaysOff = daysOffDefaultLimits.SickLeaveDaysOff;

            return Task.CompletedTask;
        }
    }
}
