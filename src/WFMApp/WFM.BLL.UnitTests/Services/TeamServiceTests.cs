using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.BLL.Services;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories.Interfaces;
using WFM.Models.Requests.TeamRequests;

namespace WFM.BLL.UnitTests.Services
{
    public class TeamServiceTests
    {
        private Mock<IUserRepository> _userRepositoryStub;
        private Mock<ITeamRepository> _teamRepositoryMock;
        private Mock<IDateTimeProvider> _dateTimeProvider;
        string teamName;
        private Team fakeTeam;
        private ITeamService sut;

        public TeamServiceTests()
        {
            _userRepositoryStub = new Mock<IUserRepository>();
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();
            sut = new TeamService(_teamRepositoryMock.Object, _userRepositoryStub.Object, _dateTimeProvider.Object);
        }

        [Fact]
        public async Task CreateTeam_ExistingTeam_ReturnsNull()
        {
            //arrange           
            _teamRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(new Team());

            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid()
            };

            //act
            var result = await sut.CreateAsync(fakeTeam, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTeam_NotExistingTeamLeader_ReturnsNull()
        {
            //arrange           
            _teamRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(() => null);

            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid()
            };

            //act
            var result = await sut.CreateAsync(fakeTeam, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTeam_ValidTeam_ReturnsTeam()
        {
            //arrange
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _teamRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _teamRepositoryMock.Setup(x => x.AddTeamAsync(It.IsAny<Team>())).ReturnsAsync(fakeTeam);
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());

            //act
            var result = await sut.CreateAsync(fakeTeam, Guid.NewGuid());

            //assert
            Assert.Equal(result.TeamName, fakeTeam.TeamName);
        }

        [Fact]
        public async Task CreateTeam_ValidTeam_CallsAddTeamAsync()
        {
            //arrange
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _teamRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            _teamRepositoryMock.Setup(x => x.AddTeamAsync(fakeTeam)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.CreateAsync(fakeTeam, Guid.NewGuid());

            //assert
            _teamRepositoryMock.Verify(mock => mock.AddTeamAsync(It.Is<Team>(t => t.TeamName == fakeTeam.TeamName)), Times.Once);
        }

        [Fact]
        public async Task DeleteTeam_ValidTeam_ReturnsTeamAndCallsOnce()
        {
            //arrange
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _teamRepositoryMock.Setup(x => x.DeleteTeamAsync(fakeTeam)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.DeleteAsync(fakeTeam, Guid.NewGuid());

            //assert
            _teamRepositoryMock.Verify(x => x.DeleteTeamAsync(It.Is<Team>(x => x.TeamName == fakeTeam.TeamName)), Times.Once);
        }


        [Fact]
        public async Task UpdateTeam_NotExistingTeamLeader_ReturnsNull()
        {
            //arrange
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(() => null);

            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid()
            };

            //act
            var result = await sut.UpdateAsync(fakeTeam, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTeam_ValidTeam_ReturnsTeam()
        {
            //arrange
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            _teamRepositoryMock.Setup(x => x.UpdateTeamAsync(It.IsAny<Team>(), It.IsAny<Guid>())).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.UpdateAsync(fakeTeam, Guid.NewGuid());

            //assert
            Assert.Equal(result.TeamName, fakeTeam.TeamName);
        }

        [Fact]
        public async Task GetByName_ValidName_ReturnsTeam()
        {
            //arrange
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };
            teamName = "test";

            _teamRepositoryMock.Setup(x => x.GetByNameAsync(teamName)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.GetByNameAsync(teamName);

            //assert
            _teamRepositoryMock.Verify(x => x.GetByNameAsync(It.Is<string>(x => x.Contains(result.TeamName))), Times.Once);
        }

        [Fact]
        public async Task GetByName_InvalidName_ReturnsNull()
        {
            //arrange
            teamName = "test";

            //act
            var result = await sut.GetByNameAsync(teamName);

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_Default_CallsGetAllAsync()
        {
            //arrange
            _teamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Team>());

            //act
            var result = await sut.GetAllAsync();

            //assert
            _teamRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_Default_ShoulReturnCollectionOfTeams()
        {
            //arrange
            var teams = new List<Team>
            {
                new Team { TeamName = "test"},
                new Team { TeamName = "test2"}
            };

            //arrange
            _teamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(teams);

            //act
            var result = await sut.GetAllAsync();

            //assert
            Assert.Equal(teams, result);
        }

        [Fact]
        public async Task GetById_ValidGuid_ReturnsTeam()
        {
            //arrange
            fakeTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _teamRepositoryMock.Setup(x => x.GetByIdAsync(fakeTeam.Id)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.GetByIdAsync(fakeTeam.Id);

            //assert
            _teamRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<Guid>(x => x.Equals(fakeTeam.Id))), Times.Once);
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsNull()
        {
            //arrange
            Guid guid = new Guid();

            //act
            var result = await sut.GetByIdAsync(guid);

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMember_NotExistingUser_ReturnsNull()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO()
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
            _userRepositoryStub.Setup(x => x.FindById(teamAddRemoveMemberDTO.UserId)).ReturnsAsync(() => null);
           
            //act
            var result = await sut.AddMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMember_NotExistingTeam_ReturnsNull()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO()
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _teamRepositoryMock.Setup(x => x.GetByIdAsync(teamAddRemoveMemberDTO.TeamId)).ReturnsAsync(() => null);
            _userRepositoryStub.Setup(x => x.FindById(teamAddRemoveMemberDTO.UserId)).ReturnsAsync(() => new User());

            //act
            var result = await sut.AddMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMember_ValidTeamAndUser_ReturnsTeamMembersIncreases()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();
            
            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            _teamRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(fakeTeam);
            _teamRepositoryMock.Setup(x => x.AddTeamMember(It.IsAny<Team>())).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.AddMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Equal(1, result.TeamMembers.Count);
        }

        [Fact]
        public async Task AddMember_ValidTeamAndUser_CallsUpdateTeamAsync()
        {
            //arrange
            fakeTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();

            _teamRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(fakeTeam);
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());
            _teamRepositoryMock.Setup(x => x.AddTeamMember(fakeTeam)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.AddMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            _teamRepositoryMock.Verify(mock => mock.AddTeamMember(It.Is<Team>(t => t.TeamName == fakeTeam.TeamName)), Times.Once);
        }

        [Fact]
        public async Task RemoveMember_NotExistingTeam_ReturnsNull()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO()
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
            _teamRepositoryMock.Setup(x => x.GetByIdWithMembers(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //act
            var result = await sut.RemoveMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }


        [Fact]
        public async Task RemoveMember_NotExistingUser_ReturnsNull()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO()
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _teamRepositoryMock.Setup(x => x.GetByIdWithMembers(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(new Team());
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //act
            var result = await sut.RemoveMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveMember_ValidTeamAndUser_ReturnsDescreasedTeamMembers()
        {
            //arrange
            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO();
            User user = new User
            {
                UserName = "test"
            };

            fakeTeam = new Team
            {
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test",                
            };

            fakeTeam.TeamMembers.Add(user);

            _teamRepositoryMock.Setup(x => x.GetByIdWithMembers(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(fakeTeam);
            _userRepositoryStub.Setup(x => x.FindById(It.IsAny<Guid>())).ReturnsAsync(user);

            _teamRepositoryMock.Setup(x => x.UpdateTeamAsync(fakeTeam, It.IsAny<Guid>())).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.RemoveMemberAsync(teamAddRemoveMemberDTO, Guid.NewGuid());

            //assert
            Assert.Equal(0, result.TeamMembers.Count);
        }

        [Fact]
        public async Task RemoveMember_ValidTeamAndUser_CallsUpdateTeamAsync()
        {
            //arrange
            fakeTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = "test",
                TeamLeaderId = Guid.NewGuid(),
                Description = "test"
            };

            User user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "test"
            };

            TeamAddRemoveMemberDTO teamAddRemoveMemberDTO = new TeamAddRemoveMemberDTO()
            {
                TeamId = fakeTeam.Id,
                UserId = user.Id
            };

            _teamRepositoryMock.Setup(x => x.GetByIdWithMembers(teamAddRemoveMemberDTO.TeamId, teamAddRemoveMemberDTO.UserId)).ReturnsAsync(fakeTeam);
            _userRepositoryStub.Setup(x => x.FindById(teamAddRemoveMemberDTO.UserId)).ReturnsAsync(user);
            _teamRepositoryMock.Setup(x => x.UpdateTeamAsync(fakeTeam, user.Id)).ReturnsAsync(fakeTeam);

            //act
            var result = await sut.RemoveMemberAsync(teamAddRemoveMemberDTO, user.Id);

            //assert
            _teamRepositoryMock.Verify(mock => mock.UpdateTeamAsync(It.Is<Team>(t => t.TeamName == fakeTeam.TeamName), It.Is<Guid>(x => x.Equals(user.Id))), Times.Once);
        }
    }
}
