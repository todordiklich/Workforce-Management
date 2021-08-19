using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WFM.BLL.Interfaces;
using WFM.DAL.Entities;
using WFM.Models.DTO.Requests.TimeOffRequestRequests;
using WFM.Models.Requests.UserRequests;
using WFM.WEB.Controllers;
using Xunit;

namespace WFM.WEB.UnitTests.Controllers
{
    public class TimeOffRequestsControllerTests
    {
        private Mock<ITimeOffRequestService> timeOffRequestServiceMock;
        private Mock<IApprovalService> approvalServiceMock;
        private Mock<IMapper> mapperMock;
        private Mock<IUserService> userServiceMock;

        private TimeOffRequestsController sut;

        public TimeOffRequestsControllerTests()
        {
            timeOffRequestServiceMock = new Mock<ITimeOffRequestService>();
            approvalServiceMock = new Mock<IApprovalService>();
            mapperMock = new Mock<IMapper>();
            userServiceMock = new Mock<IUserService>();

            sut = new TimeOffRequestsController(timeOffRequestServiceMock.Object, approvalServiceMock.Object, mapperMock.Object, userServiceMock.Object);
        }

        private void SetClaimPrincipal()
        {
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAllMyRequests_Default_ReturnsOk()
        {
            //arrange
            SetClaimPrincipal();
            timeOffRequestServiceMock.Setup(ts => ts.GetAllMyRequestsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<TimeOffRequest>());

            //act
            var result = await sut.GetAll();

            //assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllMyRequestApprovals_Default_ReturnsOk()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            SetClaimPrincipal();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(aps => aps.GetAllRequestApprovalsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Approval>());

            //act
            var result = await sut.Get(requestId);

            //assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllMyRequestApprovals_TimeOffRequestIsNull_ReturnsBadRequest()
        {
            //arrange
            Guid userId = Guid.NewGuid();
            Guid requestId = Guid.NewGuid();
            SetClaimPrincipal();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TimeOffRequest)null);
            approvalServiceMock.Setup(aps => aps.GetAllRequestApprovalsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Approval>());

            //act
            var result = await sut.Get(requestId);

            //assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Post_CanNotCreateTimeOffRequest_ReturnsBadRequest()
        {
            //arrange
            SetClaimPrincipal();

            TimeOffRequestCreateDTO requestCreateDto = new TimeOffRequestCreateDTO();
            timeOffRequestServiceMock.Setup(ts => ts.CreateAsync(It.IsAny<TimeOffRequest>(), It.IsAny<Guid>()))
                .ReturnsAsync((TimeOffRequest) null);

            //act
            var result = await sut.Post(requestCreateDto);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Post_WorksProperly_ReturnsCreatedAtAction()
        {
            //arrange
            timeOffRequestServiceMock.Setup(us => us.CreateAsync(It.IsAny<TimeOffRequest>(), It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());

            SetClaimPrincipal();

            TimeOffRequestCreateDTO requestCreateDto = new TimeOffRequestCreateDTO();

            //act
            var result = await sut.Post(requestCreateDto);

            //assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Put_PassInvalidRequestId_ReturnsNotFound()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            TimeOffRequestEditDTO requestDTO = new TimeOffRequestEditDTO();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TimeOffRequest)null);
            //act
            var result = await sut.Put(requestId, requestDTO);

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Put_WorksProperly_ReturnsNoContent()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new TimeOffRequest());
            timeOffRequestServiceMock.Setup(ts => ts.UpdateAsync(It.IsAny<TimeOffRequest>(), It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());

            SetClaimPrincipal();

            TimeOffRequestEditDTO requestDTO = new TimeOffRequestEditDTO();

            //act
            var result = await sut.Put(requestId, requestDTO);

            //assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_PassInvalidRequestId_ReturnsNotFound()
        {
            //act
            var result = await sut.Delete(Guid.NewGuid());

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WorksProperly_ReturnsNoContent()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());
            timeOffRequestServiceMock.Setup(us => us.DeleteAsync(It.IsAny<TimeOffRequest>(), It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());

            SetClaimPrincipal();

            //act
            var result = await sut.Delete(requestId);

            //assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetAllMyAssignedRequests_Default_ReturnsOk()
        {
            //arrange
            timeOffRequestServiceMock.Setup(ts => ts.GetAllMyAssignedRequestsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<TimeOffRequest>());
            SetClaimPrincipal();

            //act
            var result = await sut.GetAllAssigned();

            //assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Approve_WorksProperly_ReturnsApproval()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(aps => aps.FindByUserIdAndRequestId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Approval());
            approvalServiceMock.Setup(aps => aps.ApproveAsync(It.IsAny<Approval>()))
                .ReturnsAsync(new Approval());
            SetClaimPrincipal();

            //act
            var result = await sut.Approve(requestId);

            //assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Approve_TimeOffRequestIsNull_ReturnsBadRequest()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TimeOffRequest)null);
            SetClaimPrincipal();

            //act
            var result = await sut.Approve(requestId);

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Reject_WorksProperly_ReturnsApproval()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TimeOffRequest());
            approvalServiceMock.Setup(aps => aps.FindByUserIdAndRequestId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Approval());
            approvalServiceMock.Setup(aps => aps.RejectAsync(It.IsAny<Approval>()))
                .ReturnsAsync(new Approval());
            SetClaimPrincipal();

            //act 
            var result = await sut.Reject(requestId);

            //assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Reject_TimeOffRequestIsNull_ReturnsBadRequest()
        {
            //arrange
            Guid requestId = Guid.NewGuid();
            timeOffRequestServiceMock.Setup(ts => ts.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TimeOffRequest)null);
            SetClaimPrincipal();

            //act
            var result = await sut.Reject(requestId);

            //assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
