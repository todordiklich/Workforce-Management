using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WFM.BLL.Interfaces;
using WFM.Common.CustomExceptions;
using WFM.DAL.Entities;
using WFM.Models.DTO.Responses;
using WFM.Models.DTO.Responses.UserResponses;


namespace WFM.WEB.Controllers
{
    [ApiController]
    [Route("Test")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public TestController(IEmailService emailService, IUserService userService, IMapper mapper)
        {
            _emailService = emailService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet("Hardcoded-demo")]
        public async Task<IActionResult> SendMailDemo()
        {
            var toName = "Your Name"; //User.Username
            var toEmailAddress = "from@wpfapp.com"; //User.emailaddress
            var subject = "Day off request / response"; //Needs custom logic that depends on Approval.status
            var message = "Test mail sent successfully"; //Needs custom text that depends on Approval.status and the end receiver (user/teamleader)

            await _emailService.SendDayOffNotificationAsync(toName, toEmailAddress, subject, message);
            return Ok();
        }

        /*[HttpGet("CheckRemainingDaysOff")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckRemainingDaysResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRemainingDaysOff()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            User remainingDaysOffFromDB = await _userService.GetByIdAsync(userId);

            if (remainingDaysOffFromDB == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<CheckRemainingDaysOffResponseDTO>(remainingDaysOffFromDB));
        }*/

        [HttpGet("ThrowInternalServerError")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> TestStatusCode500()
        {
            throw new ArgumentException();
        }

        [HttpGet("ThrowBadRequest")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IActionResult> TestStatusCode400()
        {
            throw new CustomApplicationException("Throw Status Code 400 for test purposes");
        }
    }
}
