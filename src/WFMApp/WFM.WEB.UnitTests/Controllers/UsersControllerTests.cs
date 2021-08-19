using Moq;
using Xunit;
using System;
using AutoMapper;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.WEB.Controllers;
using WFM.Models.Requests.UserRequests;

namespace WFM.WEB.UnitTests.Controllers
{
    public class UsersControllerTests
    {
        private Mock<IMapper> mapperMock;
        private Mock<IUserService> userServiceMock;
        private UsersController sut;

        public UsersControllerTests()
        {
            mapperMock = new Mock<IMapper>();
            userServiceMock = new Mock<IUserService>();

            sut = new UsersController(userServiceMock.Object, mapperMock.Object);
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
        public async Task Get_Default_ReturnsOk()
        {
            //arrange
            Guid userId = Guid.NewGuid();
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User() { Id = userId });

            //act
            var result = await sut.Get(userId);

            //assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Get_PassInvalidId_ReturnsBadRequest()
        {
            //act
            var result = await sut.Get(Guid.NewGuid());

            //assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_Default_ReturnsOk()
        {
            //arrange
            userServiceMock.Setup(us => us.GetAllAsync()).ReturnsAsync(new List<User>());

            //act
            var result = await sut.GetAll();

            //assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Post_CanNotCreateUser_ReturnsBadRequest()
        {
            //arrange
            SetClaimPrincipal();

            UserCreateRequestDTO userDTO = new UserCreateRequestDTO();

            //act
            var result = await sut.Post(userDTO);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Post_CanNotAddUserToRole_ReturnsBadRequest()
        {
            //arrange
            userServiceMock.Setup(us => us.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(true);

            SetClaimPrincipal();

            UserCreateRequestDTO userDTO = new UserCreateRequestDTO();

            //act
            var result = await sut.Post(userDTO);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Post_WorksProperly_ReturnsCreatedAtAction()
        {
            //arrange
            userServiceMock.Setup(us => us.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(true);

            userServiceMock.Setup(us => us.AddUserToRole(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);

            userServiceMock.Setup(us => us.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = Guid.NewGuid()});

            SetClaimPrincipal();

            UserCreateRequestDTO userDTO = new UserCreateRequestDTO();

            //act
            var result = await sut.Post(userDTO);

            //assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Put_PassInvalidUserId_ReturnsNotFound()
        {
            //arrange
            UserEditRequestDTO userDTO = new UserEditRequestDTO();

            //act
            var result = await sut.Put(userDTO);

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Put_PassInvalidRole_ReturnsBadRequest()
        {
            //arrange
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());

            UserEditRequestDTO userDTO = new UserEditRequestDTO();

            //act
            var result = await sut.Put(userDTO);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Put_PassInvalidCountryOfResidence_ReturnsBadRequest()
        {
            //arrange
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());
            userServiceMock.Setup(us => us.CheckIfRoleExists(It.IsAny<string>())).ReturnsAsync(true);

            UserEditRequestDTO userDTO = new UserEditRequestDTO();

            //act
            var result = await sut.Put(userDTO);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Put_WorksProperly_ReturnsBadRequest()
        {
            //arrange
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());
            userServiceMock.Setup(us => us.CheckIfRoleExists(It.IsAny<string>())).ReturnsAsync(true);
            userServiceMock.Setup(us => us.CheckIfCountryOfResidenceExists(It.IsAny<string>())).ReturnsAsync(true);

            SetClaimPrincipal();

            UserEditRequestDTO userDTO = new UserEditRequestDTO();

            //act
            var result = await sut.Put(userDTO);

            //assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_PassInvalidUserId_ReturnsNotFound()
        {
            //act
            var result = await sut.Delete(Guid.NewGuid());

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_UserIsNotDeleted_ReturnsBadRequest()
        {
            //arrange
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());

            //act
            var result = await sut.Delete(Guid.NewGuid());

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Delete_WorksProperly_ReturnsNoContent()
        {
            //arrange
            userServiceMock.Setup(us => us.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());
            userServiceMock.Setup(us => us.DeleteAsync(It.IsAny<User>())).ReturnsAsync(true);

            //act
            var result = await sut.Delete(Guid.NewGuid());

            //assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
