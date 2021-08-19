using System;
using Nager.Date;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Enums;
using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.Common.CustomExceptions;
using WFM.DAL.Repositories.Interfaces;

namespace WFM.BLL.Services
{
    public class TimeOffRequestService : ITimeOffRequestService
    {
        private readonly ITimeOffRequestRepository _timeOffRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHolidayService _holidayService;
        private readonly IApprovalService _approvalService;
        private readonly IEmailService _emailService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TimeOffRequestService(
            ITimeOffRequestRepository timeOffRequestRepository,
            IUserRepository userRepository,
            IHolidayService holidayService,
            IApprovalService approvalService,
            IEmailService emailService,
            IDateTimeProvider dateTimeProvider)
        {
            _timeOffRequestRepository = timeOffRequestRepository;
            _userRepository = userRepository;
            _holidayService = holidayService;
            _approvalService = approvalService;
            _emailService = emailService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<TimeOffRequest>> GetAllMyRequestsAsync(Guid userId)
        {
            return await _timeOffRequestRepository.GetAllMyRequestsAsync(userId);
        }

        public async Task<List<TimeOffRequest>> GetAllMyAssignedRequestsAsync(Guid userId)
        {
            return await _timeOffRequestRepository.GetAllMyAssignedRequestsAsync(userId);
        }

        public async Task<TimeOffRequest> FindByIdAsync(Guid requestId)
        {
            return await _timeOffRequestRepository.FindByIdAsync(requestId);
        }

        public async Task<TimeOffRequest> CreateAsync(TimeOffRequest timeOffRequest, Guid userId)
        {
            var user = await _userRepository.FindById(userId);
            if (user.TimeOffRequests.Any(r => (r.StartDate <= timeOffRequest.StartDate &&
                               r.EndDate >= timeOffRequest.EndDate) ||
                              (r.EndDate >= timeOffRequest.StartDate &&
                               r.EndDate <= timeOffRequest.EndDate) ||
                              (r.StartDate >= timeOffRequest.StartDate &&
                               r.StartDate <= timeOffRequest.EndDate))
            )
            {
                throw new CustomApplicationException("Your time off request days overlap with another time off request you have submitted.");
            }

            var admin = await _userRepository.FindByName("admin");

            //calculate requested days
            var requestedDays = await CalculateRequestedDays(timeOffRequest, user);

            timeOffRequest.User = user;
            timeOffRequest.CreatedBy = user.Id;
            timeOffRequest.RequestStatus = TimeOffStatus.Created;
            timeOffRequest.RequestedDays = requestedDays;

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(userId);
            List<User> userTeamMembers = user.Teams.SelectMany(t => t.TeamMembers).Where(u => u.Id != user.Id).Distinct().ToList();

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(user);
            allEmailRecipients.Add(admin);

            List<User> allEmailRecipientsWithMembers = new List<User>();
            allEmailRecipientsWithMembers.AddRange(allEmailRecipients);
            allEmailRecipientsWithMembers.AddRange(userTeamMembers);

            //handle sick leave/paid/unpaid
            if (timeOffRequest.RequestType == TimeOffReason.SickLeave)
            {
                user.AvailableSickLeaveDaysOff -= requestedDays;
                timeOffRequest.RequestStatus = TimeOffStatus.Approved;

                if (requestedDays > user.AvailableSickLeaveDaysOff)
                {
                    var subject = $"DAYSOFF: WARNING! User {user.UserName} - {user.FirstName} {user.LastName}, has exceeded available Sick Leave Days Off.";
                    var message = timeOffRequest.Description;
                    foreach (var member in allEmailRecipients)
                    {
                        var toName = member.FirstName + " " + member.LastName;
                        var toEmailAddress = member.Email;

                        await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                    }
                }
                else
                {
                    var subject = $"DAYSOFF: User {user.UserName} - {user.FirstName} {user.LastName}," +
                                  $" has requested Sick Leave Days Off from {timeOffRequest.StartDate} to {timeOffRequest.EndDate} .";
                    var message = timeOffRequest.Description;
                    foreach (var member in allEmailRecipientsWithMembers)
                    {
                        var toName = member.FirstName + " " + member.LastName;
                        var toEmailAddress = member.Email;

                        await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                    }
                }

                timeOffRequest.ModifiedOn = _dateTimeProvider.UtcNow;
                return await _timeOffRequestRepository.UpdateTimeOffRequestAsync(timeOffRequest);
            }

            if (timeOffRequest.RequestType == TimeOffReason.Paid)
            {
                if (requestedDays > user.AvailablePaidDaysOff)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} paid days off.You currently have {user.AvailablePaidDaysOff} paid days off remaining");
                }

                user.AvailablePaidDaysOff -= requestedDays;
            }
            else if (timeOffRequest.RequestType == TimeOffReason.Unpaid)
            {
                if (requestedDays > user.AvailableUnpaidDaysOff)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} days off.You currently have {user.AvailablePaidDaysOff} days off remaining");
                }

                user.AvailableUnpaidDaysOff -= requestedDays;
            }

            //Create request and get Id in order to create approval
            var dateTimeNow = _dateTimeProvider.UtcNow;
            timeOffRequest.CreatedOn = dateTimeNow;
            timeOffRequest.ModifiedOn = dateTimeNow;
            TimeOffRequest createdTimeOffRequest = await _timeOffRequestRepository.AddTimeOffRequestAsync(timeOffRequest);

            //if user has no team leaders the request is auto approved
            if (!userTeamLeaders.Any())
            {
                createdTimeOffRequest.RequestStatus = TimeOffStatus.Approved;

                var subject = $"DAYSOFF: APPROVED! User {user.UserName} - {user.FirstName} {user.LastName}'s " +
                              $"request for {timeOffRequest.RequestType} days off from {timeOffRequest.StartDate} to {timeOffRequest.EndDate} has been approved.";
                var message = timeOffRequest.Description;
                var toName = user.FirstName + " " + user.LastName;
                var toEmailAddress = user.Email;

                await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);

                timeOffRequest.ModifiedOn = _dateTimeProvider.UtcNow;
                return await _timeOffRequestRepository.UpdateTimeOffRequestAsync(timeOffRequest);
            }

            await CreateApprovalsForTeamLeaders(userId, userTeamLeaders, createdTimeOffRequest);

            //if all approved, approve request
            var requestApprovals = await _approvalService.GetAllRequestApprovalsAsync(createdTimeOffRequest.Id);

            if (requestApprovals.All(a => a.ApprovalStatus == TimeOffApprovalStatus.Approved))
            {
                createdTimeOffRequest.RequestStatus = TimeOffStatus.Approved;

                var subject = $"DAYSOFF: User {user.UserName} - {user.FirstName} {user.LastName}'s " +
                              $"request for {timeOffRequest.RequestType} days off from {timeOffRequest.StartDate} to {timeOffRequest.EndDate} has been approved.";
                var message = timeOffRequest.Description;
                foreach (var member in allEmailRecipients)
                {
                    var toName = member.FirstName + " " + member.LastName;
                    var toEmailAddress = member.Email;

                    await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                }

                timeOffRequest.ModifiedOn = _dateTimeProvider.UtcNow;
                return await _timeOffRequestRepository.UpdateTimeOffRequestAsync(createdTimeOffRequest);
            }
            else
            {
                var subject = $"DAYSOFF: User {user.UserName} - {user.FirstName} {user.LastName}'s request for {timeOffRequest.RequestType}" +
                              $" days off from {timeOffRequest.StartDate} to {timeOffRequest.EndDate} has been submitted for approval.";
                var message = timeOffRequest.Description;
                foreach (var member in allEmailRecipients)
                {
                    var toName = member.FirstName + " " + member.LastName;
                    var toEmailAddress = member.Email;

                    await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                }
            }

            timeOffRequest.ModifiedOn = _dateTimeProvider.UtcNow;
            return await _timeOffRequestRepository.UpdateTimeOffRequestAsync(createdTimeOffRequest);
        }

        private async Task CreateApprovalsForTeamLeaders(Guid userId, List<User> userTeamLeaders,
            TimeOffRequest createdTimeOffRequest)
        {
            foreach (var userTeamLeader in userTeamLeaders)
            {
                var approval = CreateTeamLeaderApproval(userId, createdTimeOffRequest, userTeamLeader);

                await _approvalService.CreateAsync(approval);
            }
        }

        private static Approval CreateTeamLeaderApproval(Guid userId, TimeOffRequest createdTimeOffRequest, User userTeamLeader)
        {
            var isTeamLeaderOff = userTeamLeader.TimeOffRequests.Any(r => r.RequestStatus == TimeOffStatus.Approved &&
                                                                          r.StartDate <= createdTimeOffRequest.CreatedOn &&
                                                                          r.EndDate > createdTimeOffRequest.CreatedOn);

            Approval approval;
            if (userTeamLeader.Id == userId || isTeamLeaderOff)
            {
                approval = new Approval()
                {
                    TeamLeaderId = userTeamLeader.Id,
                    TimeOffRequestId = createdTimeOffRequest.Id,
                    ApprovalStatus = TimeOffApprovalStatus.Approved
                };
            }
            else
            {
                approval = new Approval()
                {
                    TeamLeaderId = userTeamLeader.Id,
                    TimeOffRequestId = createdTimeOffRequest.Id,
                    ApprovalStatus = TimeOffApprovalStatus.Awaiting
                };
            }

            return approval;
        }

        public async Task<TimeOffRequest> UpdateAsync(TimeOffRequest timeOffRequest, Guid userId)
        {
            if (timeOffRequest.RequestStatus != TimeOffStatus.Created)
            {
                throw new CustomApplicationException("You can't update a time off request that is already approved or being processed.");
            }

            var user = await _userRepository.FindById(userId);
            if (user.TimeOffRequests.Where(r => r.Id != timeOffRequest.Id)
                                    .Any(r => (r.StartDate <= timeOffRequest.StartDate &&
                                               r.EndDate >= timeOffRequest.EndDate) ||
                                              (r.EndDate >= timeOffRequest.StartDate &&
                                               r.EndDate <= timeOffRequest.EndDate) ||
                                              (r.StartDate >= timeOffRequest.StartDate &&
                                               r.StartDate <= timeOffRequest.EndDate))
            )
            {
                throw new CustomApplicationException("Your time off request days overlap with another time off request you have submitted.");
            }

            var admin = await _userRepository.FindByName("admin");

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(userId);

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(user);
            allEmailRecipients.Add(admin);

            var requestedDays = await CalculateRequestedDays(timeOffRequest, user);

            if (timeOffRequest.RequestType == TimeOffReason.Paid)
            {
                if ((user.AvailablePaidDaysOff + timeOffRequest.RequestedDays) - requestedDays < 0)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} paid days off.You currently have {user.AvailablePaidDaysOff + timeOffRequest.RequestedDays} paid days off remaining");
                }
                user.AvailablePaidDaysOff += timeOffRequest.RequestedDays;
            }
            else if (timeOffRequest.RequestType == TimeOffReason.Unpaid)
            {
                if ((user.AvailableUnpaidDaysOff + timeOffRequest.RequestedDays) - requestedDays < 0)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} paid days off.You currently have {user.AvailableUnpaidDaysOff + timeOffRequest.RequestedDays} paid days off remaining");
                }
                user.AvailableUnpaidDaysOff += timeOffRequest.RequestedDays;
            }

            if (timeOffRequest.RequestType == TimeOffReason.Paid)
            {
                if (requestedDays > user.AvailablePaidDaysOff)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} paid days off.You currently have {user.AvailablePaidDaysOff} paid days off remainig");
                }

                user.AvailablePaidDaysOff -= requestedDays;
            }
            else if (timeOffRequest.RequestType == TimeOffReason.Unpaid)
            {
                if (requestedDays > user.AvailableUnpaidDaysOff)
                {
                    throw new CustomApplicationException($"You cannot request {requestedDays} days off.You currently have {user.AvailablePaidDaysOff} days off remainig");
                }

                user.AvailableUnpaidDaysOff -= requestedDays;
            }

            var subject =
                $"DAYSOFF: User {user.UserName} - {user.FirstName} {user.LastName}'s request for {timeOffRequest.RequestType}" +
                $" days off duration has been changed to last from {timeOffRequest.StartDate} to {timeOffRequest.EndDate}.";
            var message = timeOffRequest.Description;
            foreach (var member in allEmailRecipients)
            {
                var toName = member.FirstName + " " + member.LastName;
                var toEmailAddress = member.Email;

                await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
            }

            timeOffRequest.RequestedDays = requestedDays;
            timeOffRequest.ModifiedBy = userId;
            timeOffRequest.ModifiedOn = _dateTimeProvider.UtcNow;

            return await _timeOffRequestRepository.UpdateTimeOffRequestAsync(timeOffRequest);
        }

        public async Task<TimeOffRequest> DeleteAsync(TimeOffRequest timeOffRequest, Guid userId)
        {
            var user = await _userRepository.FindById(userId);
            var admin = await _userRepository.FindByName("admin");

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(userId);

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(user);
            allEmailRecipients.Add(admin);

            if (timeOffRequest.RequestStatus == TimeOffStatus.Rejected)
            {
                throw new CustomApplicationException("You can not delete rejected requests.");
            }

            if (timeOffRequest.RequestType == TimeOffReason.SickLeave)
            {
                user.AvailableSickLeaveDaysOff += timeOffRequest.RequestedDays;
            }
            else if (timeOffRequest.RequestType == TimeOffReason.Paid)
            {
                user.AvailablePaidDaysOff += timeOffRequest.RequestedDays;
            }
            else if (timeOffRequest.RequestType == TimeOffReason.Unpaid)
            {
                user.AvailableUnpaidDaysOff += timeOffRequest.RequestedDays;
            }

            user.ModifiedBy = userId;
            await _userRepository.Update(user);

            timeOffRequest.ModifiedBy = userId;

            var subject =
                $"DAYSOFF: WARNING! User {user.UserName} - {user.FirstName} {user.LastName}'s request for {timeOffRequest.RequestType}" +
                $" days off from {timeOffRequest.StartDate} to {timeOffRequest.EndDate} has been deleted.";
            var message = timeOffRequest.Description;
            foreach (var member in allEmailRecipients)
            {
                var toName = member.FirstName + " " + member.LastName;
                var toEmailAddress = member.Email;

                await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
            }

            timeOffRequest.IsDeleted = true;
            timeOffRequest.DeletedOn = _dateTimeProvider.UtcNow;
            return await _timeOffRequestRepository.DeleteTimeOffRequestAsync(timeOffRequest);
        }

        public async Task<bool> CheckIfUserIsOwner(Guid timeOffRequestId, User user)
        {
            return await _timeOffRequestRepository.CheckIfUserIsOwner(timeOffRequestId, user);
        }

        public async Task<int> CancelUserTimeOffRequests(Guid userId)
        {
            List<TimeOffRequest> requests = await GetAllMyRequestsAsync(userId);

            foreach (var request in requests)
            {
                request.RequestStatus = TimeOffStatus.Canceled;

                await _timeOffRequestRepository.UpdateTimeOffRequestAsync(request);
            }

            return requests.Where(r => r.RequestType == TimeOffReason.Paid).Sum(r => r.RequestedDays);
        }

        private async Task<int> CalculateRequestedDays(TimeOffRequest timeOffRequest, User user)
        {
            int requestedDays;
            bool success = Enum.TryParse<CountryCode>(user.CountryOfResidence, out var countryCode);
            if (success)
            {
                requestedDays =
                    await _holidayService.DaysTimeOff(timeOffRequest.StartDate, timeOffRequest.EndDate, countryCode);
            }
            else
            {
                throw new CustomApplicationException("Country code does not exist.");
            }

            return requestedDays;
        }
    }
}
