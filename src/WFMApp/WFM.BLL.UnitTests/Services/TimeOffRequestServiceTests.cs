using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Moq;
using Nager.Date;
using WFM.BLL.Interfaces;
using WFM.BLL.Services;
using WFM.Common.CustomExceptions;
using WFM.DAL.Entities;
using WFM.DAL.Enums;
using WFM.DAL.Repositories.Interfaces;
using Xunit;

namespace WFM.BLL.UnitTests.Services
{
    public class TimeOffRequestServiceTests
    {
        private Mock<ITimeOffRequestRepository> timeOffRequestRepositoryMock;
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IHolidayService> holidayServiceMock;
        private Mock<IApprovalService> approvalServiceMock;
        private Mock<IEmailService> emailServiceMock;
        private Mock<IDateTimeProvider> dateTimeProviderServiceMock;
        private TimeOffRequestService sut;

        public TimeOffRequestServiceTests()
        {
            timeOffRequestRepositoryMock = new Mock<ITimeOffRequestRepository>();
            userRepositoryMock = new Mock<IUserRepository>();
            holidayServiceMock = new Mock<IHolidayService>();
            approvalServiceMock = new Mock<IApprovalService>();
            emailServiceMock = new Mock<IEmailService>();
            dateTimeProviderServiceMock = new Mock<IDateTimeProvider>();

            sut = new TimeOffRequestService(
                timeOffRequestRepositoryMock.Object,
                userRepositoryMock.Object,
                holidayServiceMock.Object,
                approvalServiceMock.Object,
                emailServiceMock.Object,
                dateTimeProviderServiceMock.Object);
        }

        [Fact]
        public async Task FindByIdAsync_WorksProperly_ReturnsTimeOffRequest()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestRepositoryMock.Setup(tr => tr.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());

            //act
            var result = await sut.FindByIdAsync(requestId);
            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task FindByIdAsync_WrongRequestId_ReturnsNull()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestRepositoryMock.Setup(tr => tr.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TimeOffRequest) null);

            //act
            var result = await sut.FindByIdAsync(requestId);
            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WorksProperlySickLeave_ReturnsTimeOffRequest()
        {
            //arrange
            TimeOffRequest request = new TimeOffRequest(){RequestType = TimeOffReason.SickLeave};
            Guid userId = Guid.NewGuid();
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User() { CountryOfResidence = "BG" });
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());

            //act
            var result = await sut.CreateAsync(request, userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_WorksProperlyPaidLeave_ReturnsTimeOffRequest()
        {
            //arrange
            TimeOffRequest request = new TimeOffRequest() { RequestType = TimeOffReason.Paid };
            Guid userId = Guid.NewGuid();
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User() 
                { CountryOfResidence = "BG"});
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>(){new User(), new User()});
            timeOffRequestRepositoryMock.Setup(tr => tr.AddTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Approval>());

            //act
            var result = await sut.CreateAsync(request, userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_LessDaysPaidLeave_ThrowsException()
        {
            //arrange
            TimeOffRequest request = new TimeOffRequest() { RequestType = TimeOffReason.Paid, RequestedDays = 2};
            Guid userId = Guid.NewGuid();
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User()
                { CountryOfResidence = "BG" , AvailablePaidDaysOff = 1});
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>() { new User(), new User() });
            timeOffRequestRepositoryMock.Setup(tr => tr.AddTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Approval>());
            holidayServiceMock.Setup(hs =>
                    hs.DaysTimeOff(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CountryCode>()))
                .ReturnsAsync(10);

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.CreateAsync(request, userId));
        }

        [Fact]
        public async Task CreateAsync_LessDaysUnpaidLeave_ThrowsException()
        {
            //arrange
            TimeOffRequest request = new TimeOffRequest() { RequestType = TimeOffReason.Unpaid, RequestedDays = 2 };
            Guid userId = Guid.NewGuid();
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User()
                { CountryOfResidence = "BG", AvailablePaidDaysOff = 1 });
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>() { new User(), new User() });
            timeOffRequestRepositoryMock.Setup(tr => tr.AddTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Approval>());
            holidayServiceMock.Setup(hs =>
                    hs.DaysTimeOff(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CountryCode>()))
                .ReturnsAsync(10);

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.CreateAsync(request, userId));
        }

        [Fact]
        public async Task CreateAsync_WorksProperlyUnPaidLeave_ReturnsTimeOffRequest()
        {
            //arrange
            TimeOffRequest request = new TimeOffRequest() { RequestType = TimeOffReason.Unpaid };
            Guid userId = Guid.NewGuid();
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User()
                { CountryOfResidence = "BG" });
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>() { new User(), new User() });
            timeOffRequestRepositoryMock.Setup(tr => tr.AddTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Approval>());

            //act
            var result = await sut.CreateAsync(request, userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_WorksProperly_ReturnsTimeOffRequest()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User() { CountryOfResidence = "BG" });
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());

            TimeOffRequest request = new TimeOffRequest() { RequestStatus = TimeOffStatus.Created };
            Guid userId = Guid.NewGuid();

            //act
            var result = await sut.UpdateAsync(request,userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_RequestStatusIsNotCreated_ThrowsArgumentException()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            timeOffRequestRepositoryMock.Setup(tr => tr.UpdateTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());

            TimeOffRequest request = new TimeOffRequest() { RequestStatus = TimeOffStatus.Rejected };
            Guid userId = Guid.NewGuid();

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.UpdateAsync(request, userId));
        }

        [Fact]
        public async Task DeleteAsync_WorksProperly_ReturnsTimeOffRequest()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            timeOffRequestRepositoryMock.Setup(tr => tr.DeleteTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());

            TimeOffRequest request = new TimeOffRequest() { RequestStatus = TimeOffStatus.Created };
            Guid userId = Guid.NewGuid();

            //act
            var result = await sut.DeleteAsync(request, userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteAsync_RequestStatusIsRejected_ThrowsArgumentException()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            timeOffRequestRepositoryMock.Setup(tr => tr.DeleteTimeOffRequestAsync(It.IsAny<TimeOffRequest>()))
                .ReturnsAsync(new TimeOffRequest());

            TimeOffRequest request = new TimeOffRequest() { RequestStatus = TimeOffStatus.Rejected };
            Guid userId = Guid.NewGuid();

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.DeleteAsync(request, userId));
        }
    }
}
