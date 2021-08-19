using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;
using WFM.BLL.Interfaces;
using WFM.BLL.Services;
using WFM.Common.CustomExceptions;
using WFM.DAL.Entities;
using WFM.DAL.Enums;
using WFM.DAL.Repositories.Interfaces;
using Xunit;

namespace WFM.BLL.UnitTests.Services
{
    public class ApprovalServiceTest
    {
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IApprovalRepository> approvalRepositoryMock;
        private Mock<IEmailService> emailServiceMock;
        private Mock<IDateTimeProvider> dateTimeProviderMock;

        private ApprovalService sut;

        public ApprovalServiceTest()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            approvalRepositoryMock = new Mock<IApprovalRepository>();
            emailServiceMock = new Mock<IEmailService>();
            dateTimeProviderMock = new Mock<IDateTimeProvider>();

            sut= new ApprovalService(userRepositoryMock.Object, approvalRepositoryMock.Object, emailServiceMock.Object, dateTimeProviderMock.Object);
        }

        [Fact]
        public async Task FindByUserIdAndRequestIdAsync_WorksProperly_ReturnsApproval()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            approvalRepositoryMock.Setup(ar => ar.FindByUserIdAndRequestId(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(new Approval());

            //act
            var result = await sut.FindByUserIdAndRequestId(userId, requestId);
            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task FindByUserIdAndRequestIdAsync_WrongRequestId_ReturnsNull()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            approvalRepositoryMock.Setup(ar => ar.FindByUserIdAndRequestId(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Approval) null);

            //act
            var result = await sut.FindByUserIdAndRequestId(userId, requestId);
            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WorksProperly_ReturnsApproval()
        {
            //arrange
            Approval approval = new Approval();
            approvalRepositoryMock.Setup(ar => ar.AddApprovalAsync(It.IsAny<Approval>())).ReturnsAsync(new Approval());

            //act
            var result = await sut.CreateAsync(approval);
            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ApproveAsync_WorksProperly_ReturnsApproval()
        {
            //arrange
            Approval approval = new Approval() { TimeOffRequest = new TimeOffRequest() { RequestStatus = TimeOffStatus.Created, User = new User() }, TeamLeader = new User() };
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            approvalRepositoryMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Approval>());
            approvalRepositoryMock.Setup(ar => ar.AddApprovalAsync(It.IsAny<Approval>())).ReturnsAsync(new Approval());
            approvalRepositoryMock.Setup(ar => ar.UpdateApprovalAsync(It.IsAny<Approval>())).ReturnsAsync(new Approval());

            //act
            var result = await sut.ApproveAsync(approval);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ApproveAsync_RequestStatusRejected_ThrowsException()
        {
            //arrange
            Approval approval = new Approval() { TimeOffRequest = new TimeOffRequest() { RequestStatus = TimeOffStatus.Rejected, User = new User() } };

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.ApproveAsync(approval));
        }

        [Fact]
        public async Task RejectAsync_WorksProperly_ReturnsApproval()
        {
            //arrange
            Approval approval = new Approval() { TimeOffRequest = new TimeOffRequest() { RequestStatus = TimeOffStatus.Created, User = new User() }, TeamLeader = new User() };
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());
            userRepositoryMock.Setup(ur => ur.GetUserTeamLeaders(It.IsAny<Guid>())).ReturnsAsync(new List<User>());
            approvalRepositoryMock.Setup(ar => ar.GetAllRequestApprovalsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Approval>());
            approvalRepositoryMock.Setup(ar => ar.AddApprovalAsync(It.IsAny<Approval>())).ReturnsAsync(new Approval());
            approvalRepositoryMock.Setup(ar => ar.UpdateApprovalAsync(It.IsAny<Approval>())).ReturnsAsync(new Approval());

            //act
            var result = await sut.RejectAsync(approval);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RejectAsync_RequestStatusRejected_ThrowsException()
        {
            //arrange
            Approval approval = new Approval() { TimeOffRequest = new TimeOffRequest() { RequestStatus = TimeOffStatus.Rejected, User = new User() } };
            //approvalRepositoryMock.Setup(ar => ar.FindByUserIdAndRequestId(It.IsAny<Guid>(), It.IsAny<Guid>()))
            //    .ReturnsAsync(new Approval() { TimeOffRequest = new TimeOffRequest() { RequestStatus = TimeOffStatus.Rejected, User = new User() } });

            //act-assert
            await Assert.ThrowsAsync<CustomApplicationException>(() => sut.RejectAsync(approval));
        }
    }
}
