using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.BLL.Interfaces;
using WFM.Common.CustomExceptions;
using WFM.DAL.Entities;
using WFM.DAL.Enums;

using WFM.DAL.Repositories.Interfaces;

namespace WFM.BLL.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly IUserRepository _userRepository;
        private readonly IApprovalRepository _approvalRepository;
        private readonly IEmailService _emailService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ApprovalService(
            IUserRepository userRepository,
            IApprovalRepository approvalRepository,
            IEmailService emailService,
            IDateTimeProvider dateTimeProvider)
        {
            _userRepository = userRepository;
            _approvalRepository = approvalRepository;
            _emailService = emailService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<Approval>> GetAllAsync()
        {
            return await _approvalRepository.GetAllAsync();
        }

        public async Task<List<Approval>> GetAllRequestApprovalsAsync(Guid requestId)
        {
            return await _approvalRepository.GetAllRequestApprovalsAsync(requestId);
        }

        public async Task<Approval> FindByUserIdAndRequestId(Guid userId, Guid requestId)
        {
            return await _approvalRepository.FindByUserIdAndRequestId(userId, requestId);
        }

        public async Task<Approval> CreateAsync(Approval approval)
        {
            var dateTimeNow = _dateTimeProvider.UtcNow;
            approval.CreatedOn = dateTimeNow;
            approval.ModifiedOn = dateTimeNow;
            return await _approvalRepository.AddApprovalAsync(approval);
        }

        public async Task<Approval> ApproveAsync(Approval approval)
        {
            if (approval.TimeOffRequest.RequestStatus == TimeOffStatus.Rejected)
            {
                throw new CustomApplicationException("A rejected request cannot be approved.");
            }
            if (approval.TimeOffRequest.RequestStatus == TimeOffStatus.Canceled)
            {
                throw new CustomApplicationException("A canceled request cannot be approved.");
            }

            var admin = await _userRepository.FindByName("admin");
            var user = approval.TimeOffRequest.User;
            var teamLeader = approval.TeamLeader;

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(user.Id);

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(user);
            allEmailRecipients.Add(admin);

            approval.ModifiedBy = approval.TeamLeaderId;
            approval.ApprovalStatus = TimeOffApprovalStatus.Approved;

            if (approval.TimeOffRequest.RequestStatus == TimeOffStatus.Created)
            {
                approval.TimeOffRequest.RequestStatus = TimeOffStatus.Awaiting;
            }

            approval.ModifiedOn = _dateTimeProvider.UtcNow;
            var completedApproval = await _approvalRepository.UpdateApprovalAsync(approval);
            var requestApprovals = await _approvalRepository.GetAllRequestApprovalsAsync(approval.TimeOffRequestId);

            //send mail to user after team leader has approved request
            var subject =
                $"DAYSOFF: Team leader {teamLeader.UserName} - {teamLeader.FirstName} {teamLeader.LastName} has approved your {approval.TimeOffRequest.RequestType}" +
                $" days off request from {approval.TimeOffRequest.StartDate} to {approval.TimeOffRequest.EndDate}.";
            var message = approval.TimeOffRequest.Description;
            var toName = user.FirstName + " " + user.LastName;
            var toEmailAddress = user.Email;

            await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);

            //send email to everyone once the request has been approved
            if (requestApprovals.All(a => a.ApprovalStatus == TimeOffApprovalStatus.Approved))
            {
                approval.TimeOffRequest.RequestStatus = TimeOffStatus.Approved;

                 subject =
                    $"DAYSOFF: APPROVED! User {user.UserName} - {user.FirstName} {user.LastName}'s request for {approval.TimeOffRequest.RequestType}" +
                    $" days off from {approval.TimeOffRequest.StartDate} to {approval.TimeOffRequest.EndDate} has been approved.";
                 message = approval.TimeOffRequest.Description;
                foreach (var member in allEmailRecipients)
                {
                     toName = member.FirstName + " " + member.LastName;
                     toEmailAddress = member.Email;

                    await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
                }

                return await _approvalRepository.UpdateApprovalAsync(approval);
            }

            return completedApproval;
        }

        public async Task<Approval> RejectAsync(Approval approval)
        {
            if (approval.TimeOffRequest.RequestStatus == TimeOffStatus.Rejected)
            {
                throw new CustomApplicationException("A rejected request cannot be modified.");
            }
            if (approval.TimeOffRequest.RequestStatus == TimeOffStatus.Canceled)
            {
                throw new CustomApplicationException("A canceled request cannot be approved.");
            }

            var admin = await _userRepository.FindByName("admin");
            var user = approval.TimeOffRequest.User;
            var teamLeader = approval.TeamLeader;

            //setup email recipients
            List<User> userTeamLeaders = await _userRepository.GetUserTeamLeaders(user.Id);

            List<User> allEmailRecipients = new List<User>();
            allEmailRecipients.AddRange(userTeamLeaders);
            allEmailRecipients.Add(user);
            allEmailRecipients.Add(admin);

            approval.ModifiedBy = approval.TeamLeaderId;
            approval.ApprovalStatus = TimeOffApprovalStatus.Rejected;
            approval.TimeOffRequest.RequestStatus = TimeOffStatus.Rejected;

            if (approval.TimeOffRequest.RequestType == TimeOffReason.Paid)
            {
                user.AvailablePaidDaysOff += approval.TimeOffRequest.RequestedDays;
            }
            else if (approval.TimeOffRequest.RequestType == TimeOffReason.Unpaid)
            {
                user.AvailableUnpaidDaysOff += approval.TimeOffRequest.RequestedDays;
            }

            var subject =
                $"DAYSOFF: REJECTED! User {user.UserName} - {user.FirstName} {user.LastName}'s request for {approval.TimeOffRequest.RequestType}" +
                $" days off from {approval.TimeOffRequest.StartDate} to {approval.TimeOffRequest.EndDate}" +
                $" has been rejected by team leader {teamLeader.UserName} - {teamLeader.FirstName} {teamLeader.LastName}.";
            var message = approval.TimeOffRequest.Description;
            foreach (var member in allEmailRecipients)
            {
                var toName = member.FirstName + " " + member.LastName;
                var toEmailAddress = member.Email;

                await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
            }

            user.ModifiedBy = approval.TeamLeaderId;
            await _userRepository.Update(user);
            approval.ModifiedOn = _dateTimeProvider.UtcNow;
            return await _approvalRepository.UpdateApprovalAsync(approval);
        }
    }
}
