using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

using WFM.DAL.Entities;
using WFM.BLL.Services;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories.Interfaces;

namespace WFM.BLL.UnitTests.Services
{
    public class UserServiceTests
    {
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IDaysOffLimitDefaultService> daysOffLimitDefaultServiceMock;
        private Mock<RoleManager<IdentityRole<Guid>>> roleManagerMock;
        private Mock<IDateTimeProvider> dateTimeProviderMock;
        private Mock<ITimeOffRequestService> timeOffRequestService;
        private UserService sut;
        public UserServiceTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            daysOffLimitDefaultServiceMock = new Mock<IDaysOffLimitDefaultService>();
            roleManagerMock = GetRoleManagerMock<IdentityRole<Guid>>();
            dateTimeProviderMock = new Mock<IDateTimeProvider>();

            timeOffRequestService = new Mock<ITimeOffRequestService>();

            sut = new UserService(userRepositoryMock.Object, roleManagerMock.Object, daysOffLimitDefaultServiceMock.Object, dateTimeProviderMock.Object, timeOffRequestService.Object);
        }

        private Mock<RoleManager<TIdentityRole>> GetRoleManagerMock<TIdentityRole>() where TIdentityRole : IdentityRole<Guid>
        {
            return new Mock<RoleManager<TIdentityRole>>(
                    new Mock<IRoleStore<TIdentityRole>>().Object,
                    new IRoleValidator<TIdentityRole>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<ILogger<RoleManager<TIdentityRole>>>().Object);
        }

        [Fact]
        public async Task CreateAsync_PassInvalidRole_ReturnsFalse()
        {
            //arrange
            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_PassInvalidCountryOfResidence_ReturnsFalse()
        {
            //arrange
            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_PassUserNameThatAlreadyExists_ReturnsFalse()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindByName(It.IsAny<string>())).ReturnsAsync(new User());

            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault());

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_PassEmailThatAlreadyExists_ReturnsFalse()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.FindByEmail(It.IsAny<string>())).ReturnsAsync(new User());

            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault());

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_CanNotCreateUser_ReturnsFalse()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(false);

            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault());

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_PassValidData_ReturnsTrue()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);

            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault());

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            var result = await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_CanNotDeleteUser_ReturnsFalse()
        {
            //arrange
            User user = new User();

            //act
            var result = await sut.DeleteAsync(user);

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_PassValidData_ReturnsTrue()
        {
            //arrange
            userRepositoryMock.Setup(us => us.Delete(It.IsAny<User>())).ReturnsAsync(true);

            User user = new User();

            //act
            var result = await sut.DeleteAsync(user);

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetByIdAsync_PassInvalidUserId_ReturnsNull()
        {
            //arrange
            Guid userId = Guid.NewGuid();

            //act
            var result = await sut.GetByIdAsync(userId);

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_PassValidUserId_ReturnsUser()
        {
            //arrange
            userRepositoryMock.Setup(us => us.FindById(It.IsAny<Guid>())).ReturnsAsync(new User());

            Guid userId = Guid.NewGuid();

            //act
            var result = await sut.GetByIdAsync(userId);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAllAsync_Default_ReturnsListOfUsers()
        {
            //arrange
            userRepositoryMock.Setup(us => us.GetAll()).ReturnsAsync(new List<User>());

            //act
            var result = await sut.GetAllAsync();

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByNameAsync_PassInvalidUserName_ReturnsNull()
        {
            //arrange

            //act
            var result = await sut.GetByNameAsync("");

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_PassValidUserName_ReturnsUser()
        {
            //arrange
            userRepositoryMock.Setup(us => us.FindByName(It.IsAny<string>())).ReturnsAsync(new User());

            //act
            var result = await sut.GetByNameAsync("");

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CheckIfRoleExists_PassInvalidRoleName_ReturnsFalse()
        {
            //act
            var result = await sut.CheckIfRoleExists("");

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckIfRoleExists_PassValidRoleName_ReturnsTrue()
        {
            //arrange
            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            //act
            var result = await sut.CheckIfRoleExists("");

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckIfCountryOfResidenceExists_PassInvalidCountryCode_ReturnsFalse()
        {
            //act
            var result = await sut.CheckIfCountryOfResidenceExists("");

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckIfCountryOfResidenceExists_PassValidCountryCode_ReturnsTrue()
        {
            //arrange
            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault());

            //act
            var result = await sut.CheckIfCountryOfResidenceExists("");

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddUserToRole_PassInvalidRoleName_ReturnsFalse()
        {
            //act
            var result = await sut.AddUserToRole(new User(), "");

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddUserToRole_PassValidRoleName_ReturnsTrue()
        {
            //arrange
            userRepositoryMock.Setup(us => us.AddUserToRole(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            //act
            var result = await sut.AddUserToRole(new User(), "");

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_PassValidData_WorksProperly()
        {
            //arrange
            User user = new User();
            Guid modifierId = Guid.NewGuid();

            //act
            await sut.UpdateAsync(user, modifierId);

            //assert
            Assert.Equal(modifierId, user.ModifiedBy);
        }

        [Fact]
        public async Task IsUserInRole_UserIsNotInRole_ReturnsFalse()
        {
            //act
            var result = await sut.IsUserInRole(new User(), "");

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsUserInRole_UserIsInRole_ReturnsTrue()
        {
            //arrange
            userRepositoryMock.Setup(us => us.IsUserInRole(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);

            //act
            var result = await sut.IsUserInRole(new User(), "");

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task CalculateDaysOffForNewUser_PassValidData_WorksProperly()
        {
            //arrange
            userRepositoryMock.Setup(ur => ur.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(true);

            daysOffLimitDefaultServiceMock.Setup(ds => ds.FindByCountryCode(It.IsAny<string>())).ReturnsAsync(new DaysOffLimitDefault() { PaidDaysOff = 20, UnpaidDaysOff = 90, SickLeaveDaysOff = 40 });

            roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            dateTimeProviderMock.Setup(dp => dp.CurrentMonth).Returns(7);

            User user = new User();
            Guid creatorId = Guid.NewGuid();

            //act
            await sut.CreateAsync(user, "", "", creatorId);

            //assert
            Assert.Equal(10, user.AvailablePaidDaysOff);
            Assert.Equal(39, user.AvailableUnpaidDaysOff);
            Assert.Equal(40, user.AvailableSickLeaveDaysOff);
        }
    }
}
