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
    public class DeleteTimeOffRequestJob : IJob
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITimeOffRequestService _timeOffRequestService;

        public DeleteTimeOffRequestJob(IUserService userService, IEmailService emailService, IDateTimeProvider dateTimeProvider, ITimeOffRequestService timeOffRequestService)
        {
            _userService = userService;
            _emailService = emailService;
            _dateTimeProvider = dateTimeProvider;
            _timeOffRequestService = timeOffRequestService;
        }

        public async Task Execute(IJobExecutionContext jobExecutionContext)
        {
            var dataMap = jobExecutionContext.JobDetail.JobDataMap;
            var userId = dataMap.GetGuid("userId");
            var modifierId = dataMap.GetGuid("modifierId");

            //get all ids of time off requests for the user
            var allTimeOffRequests = await _timeOffRequestService.GetAllMyRequestsAsync(userId);

            //soft delete all time off requests
            foreach (var timeOffRequest in allTimeOffRequests)
            {
                await _timeOffRequestService.DeleteAsync(timeOffRequest, modifierId);
            }

            //get admin user
            User adminUser = await _userService.GetByNameAsync("admin");

            //send mail to the system that the task is completed
            var subject = $"The {nameof(DeleteTimeOffRequestJob)} was executed successfully!";
            var message = $"All Days Off requests for user {userId} were removed from the system successfully!";
            var toName = adminUser.FirstName + " " + adminUser.LastName;
            var toEmailAddress = adminUser.Email;

            await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
        }

        public static IJobDetail CreateJob(Guid userId, Guid modifierId)
        {
            string id = Guid.NewGuid().ToString();
            return JobBuilder.Create<DeleteTimeOffRequestJob>()
                .WithIdentity(id, "DeleteTimeOffRequestJob")
                .UsingJobData("userId", userId)
                .UsingJobData("modifierId", modifierId)
                .Build();
        }

        public static ITrigger GetTrigger()
        {
            string id = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow.AddMonths(6); 
            //var startTime = DateTime.UtcNow.AddSeconds(6); Uncomment for demo

            return TriggerBuilder.Create()
                .WithIdentity(id, "DeleteTimeOffRequestTrigger")
                .StartAt(startTime)
                .Build();
        }

    }
}
