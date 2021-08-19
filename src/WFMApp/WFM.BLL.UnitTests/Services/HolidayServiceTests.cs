using Xunit;
using System;
using Nager.Date;
using System.Threading.Tasks;

using WFM.BLL.Services;

namespace WFM.BLL.UnitTests.Services
{
    public class HolidayServiceTests
    {
        [Fact]
        public async Task DaysTimeOff_BGFiveDaysTimeOff_ReturnsFive()
        {
            // arrange
            DateTime startDate = new DateTime(2021, 7, 5);
            DateTime endDate = new DateTime(2021, 7, 11);

            var sut = new HolidayService();

            // act
            var result = await sut.DaysTimeOff(startDate, endDate, CountryCode.BG);

            // assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task DaysTimeOff_BGTimeOffDuring24May_ReturnsFour()
        {
            // arrange
            DateTime startDate = new DateTime(2021, 5, 24);
            DateTime endDate = new DateTime(2021, 5, 30);

            var sut = new HolidayService();

            // act
            var result = await sut.DaysTimeOff(startDate, endDate, CountryCode.BG);

            // assert
            Assert.Equal(4, result);
        }

        [Fact]
        public async Task DaysTimeOff_BGTimeOffDuring3March_ReturnsZero()
        {
            // arrange
            DateTime startDate = new DateTime(2021, 3, 3);
            DateTime endDate = new DateTime(2021, 3, 3);

            var sut = new HolidayService();

            // act
            var result = await sut.DaysTimeOff(startDate, endDate, CountryCode.BG);

            // assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task DaysTimeOff_BGStartDateAfretEndDate_ThrowsInvalidOperationException()
        {
            // arrange
            DateTime startDate = new DateTime(2021, 5, 3);
            DateTime endDate = new DateTime(2021, 4, 3);

            var sut = new HolidayService();

            // act - assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DaysTimeOff(startDate, endDate, CountryCode.BG));
        }

        [Fact]
        public async Task DaysTimeOff_BGStartDateInYear2020EndDateInYear2021_ReturnsTwelve()
        {
            // arrange
            DateTime startDate = new DateTime(2020, 12, 21);
            DateTime endDate = new DateTime(2021, 1, 10);

            var sut = new HolidayService();

            // act
            var result = await sut.DaysTimeOff(startDate, endDate, CountryCode.BG);

            // assert
            Assert.Equal(12, result);
        }
    }
}
