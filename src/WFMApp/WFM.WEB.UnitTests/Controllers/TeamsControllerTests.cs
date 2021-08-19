using Moq;
using Xunit;
using System;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.WEB.Controllers;
using WFM.Models.Requests.TeamRequests;
using System.Threading.Tasks;

namespace WFM.WEB.UnitTests.Controllers
{
    public class TeamsControllerTests
    {
        private Mock<IMapper> _mapperMock;
        private Mock<ITeamService> _teamServiceMock;
        private TeamsController sut;
        public TeamsControllerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _teamServiceMock = new Mock<ITeamService>();
            sut = new TeamsController(_teamServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Get_Default_ReturnsOk()
        {
            //arrange
            _teamServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Team());
            var sut = new TeamsController(_teamServiceMock.Object, _mapperMock.Object);

            //act
            var result = await sut.Get(It.IsAny<Guid>());

            //arrange
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Get_PassInvalidId_ReturnsBadRequest()
        {        
            //act
            var result = await sut.Get(It.IsAny<Guid>());

            //assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_Default_ReturnsOk()
        {
            //arrange
            _teamServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Team>());

            //act
            var result = await sut.GetAll();

            //arrange
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_CanNotCreateTeam_ReturnsBadRequest()
        {
            //arrange
            TeamBaseRequestDTO teamCreateRequestDTO = new TeamBaseRequestDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Create(teamCreateRequestDTO);

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Create_WorksProperly_ReturnsCreatedAtAction()
        {
            //arrange          
            TeamBaseRequestDTO teamCreateRequestDTO = new TeamBaseRequestDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.CreateAsync(It.IsAny<Team>(), It.IsAny<Guid>())).ReturnsAsync(new Team());
          
            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Create(teamCreateRequestDTO);

            //assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Edit_PassInvalidTeamId_ReturnsNotFound()
        {
            //arrange
            TeamBaseRequestDTO teamCreateRequestDTO = new TeamBaseRequestDTO();

            //act
            var result = await sut.Edit(Guid.NewGuid(),teamCreateRequestDTO);

            //arrange
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_CanNotEditTeam_ReturnsBadRequest()
        {
            //arrange
            TeamBaseRequestDTO teamCreateRequestDTO = new TeamBaseRequestDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Edit(Guid.NewGuid(), teamCreateRequestDTO);

            //arrange
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_WorksProperly_ReturnsNoContent()
        {
            //arrange
            TeamBaseRequestDTO teamCreateRequestDTO = new TeamBaseRequestDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Team());
            _teamServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Team>(),It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Edit(Guid.NewGuid(), teamCreateRequestDTO);

            //arrange
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_PassInvalidTeamId_ReturnsNotFound()
        {
            //act
            var result = await sut.Delete(Guid.NewGuid());

            //arrange
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_CanNotDeleteTeam_ReturnsBadRequest()
        {
            //arrange
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Delete(Guid.NewGuid());

            //arrange
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Delete_WorksProperly_ReturnsNoContent()
        {
            //arrange
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Team());
            _teamServiceMock.Setup(x => x.DeleteAsync(It.IsAny<Team>(), It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.Delete(Guid.NewGuid());

            //arrange
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AssignUserToATeam_CanNotAssignUserToATeam_ReturnsBadRequest()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);           

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.AssignUserToATeam(teamAddRemoveMemberDTO);

            //arrange
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task AssignUserToATeam_WorksProperly_ReturnsNoContent()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.AddMemberAsync(It.IsAny<TeamAddRemoveMemberDTO>(), It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.AssignUserToATeam(teamAddRemoveMemberDTO);

            //arrange
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveUserFromATeam_CanNotRemoveUserFromATeam_ReturnsBadRequest()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.RemoveUserFromATeam(teamAddRemoveMemberDTO);

            //arrange
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task RemoveUserFromATeam_WorksProperly_ReturnsNoContent()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();

            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            var user = new ClaimsPrincipal(claimsIdentity);

            _teamServiceMock.Setup(x => x.RemoveMemberAsync(It.IsAny<TeamAddRemoveMemberDTO>(), It.IsAny<Guid>())).ReturnsAsync(new Team());

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            //act
            var result = await sut.RemoveUserFromATeam(teamAddRemoveMemberDTO);

            //arrange
            Assert.IsType<NoContentResult>(result);
        }
    }
}
