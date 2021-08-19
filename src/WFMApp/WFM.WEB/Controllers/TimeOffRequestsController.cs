using System;
using AutoMapper;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.Models.DTO.Responses.UserResponses;
using WFM.Models.DTO.Requests.TimeOffRequestRequests;
using WFM.Models.DTO.Responses.TimeOffRequestResponses;

namespace WFM.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeOffRequestsController : ControllerBase
    {
        private readonly ITimeOffRequestService _timeOffRequestService;
        private readonly IApprovalService _approvalService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public TimeOffRequestsController(ITimeOffRequestService timeOffRequestService, IApprovalService approvalService, IMapper mapper, IUserService userService)
        {
            _timeOffRequestService = timeOffRequestService ?? throw new ArgumentNullException(nameof(timeOffRequestService));
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService)); ;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("CheckRemainingDaysOff")]
        [ProducesResponseType(typeof(List<CheckRemainingDaysOffResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CheckRemainingDaysOffResponseDTO>> CheckRemainingDaysOff()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            User remainingDaysOffFromDB = await _userService.GetByIdAsync(userId);

            if (remainingDaysOffFromDB == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<CheckRemainingDaysOffResponseDTO>(remainingDaysOffFromDB));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TimeOffRequestResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<TimeOffRequestResponseDTO>> Get(Guid id)
        {
            TimeOffRequest timeOffRequestsFromDB = await _timeOffRequestService.FindByIdAsync(id);

            if (timeOffRequestsFromDB == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<TimeOffRequestResponseDTO>(timeOffRequestsFromDB));
        }

        [HttpGet("AllMyRequests")]
        [ProducesResponseType(typeof(List<TimeOffRequestResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<TimeOffRequestResponseDTO>> GetAll()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<TimeOffRequest> timeOffRequestsFromDB = await _timeOffRequestService.GetAllMyRequestsAsync(userId);

            return Ok(_mapper.Map<IEnumerable<TimeOffRequestResponseDTO>>(timeOffRequestsFromDB));
        }

        [HttpGet("{timeOffRequestId}/Approvals")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TimeOffRequestResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApprovalsResponseDTO>> GetApprovals(Guid timeOffRequestId)
        {
            TimeOffRequest timeOffRequestFromDB = await _timeOffRequestService.FindByIdAsync(timeOffRequestId);

            if (timeOffRequestFromDB == null)
            {
                return BadRequest();
            }

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Approval> approvals = await _approvalService.GetAllRequestApprovalsAsync(timeOffRequestId);

            return Ok(_mapper.Map<IEnumerable<ApprovalsResponseDTO>>(approvals));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(TimeOffRequestCreateDTO requestCreateDto)
        {
            TimeOffRequest request = _mapper.Map<TimeOffRequest>(requestCreateDto);

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            TimeOffRequest result = await _timeOffRequestService.CreateAsync(request, userId);
            if (result == null)
            {
                return BadRequest();
            }

            return CreatedAtAction("Get", "TimeOffRequests", new { id = result.Id }, requestCreateDto);
        }

        [HttpPut("{timeOffRequestId}")]
        [Authorize(Policy = "CreatorOrAdminOnly")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Put(Guid timeOffRequestId, TimeOffRequestEditDTO timeOffRequestEditDto)
        {
            TimeOffRequest request = await _timeOffRequestService.FindByIdAsync(timeOffRequestId);
            if (request == null)
            {
                return NotFound();
            }

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            _mapper.Map(timeOffRequestEditDto, request, typeof(TimeOffRequestEditDTO), typeof(TimeOffRequest));

            await _timeOffRequestService.UpdateAsync(request, userId);

            return NoContent();
        }

        [HttpDelete("{timeOffRequestId}")]
        [Authorize(Policy = "CreatorOrAdminOnly")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid timeOffRequestId)
        {
            TimeOffRequest request = await _timeOffRequestService.FindByIdAsync(timeOffRequestId);
            if (request == null)
            {
                return NotFound();
            }

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _timeOffRequestService.DeleteAsync(request, userId);
            
            return NoContent();
        }

        [HttpGet("AllAssignedRequests")]
        [ProducesResponseType(typeof(List<TimeOffRequestResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<TimeOffRequestResponseDTO>> GetAllAssigned()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<TimeOffRequest> timeOffRequestsFromDB = await _timeOffRequestService.GetAllMyAssignedRequestsAsync(userId);

            return Ok(_mapper.Map<IEnumerable<TimeOffRequestResponseDTO>>(timeOffRequestsFromDB));
        }

        [HttpPut("Approve/{timeOffRequestId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Approve(Guid timeOffRequestId)
        {
            TimeOffRequest request = await _timeOffRequestService.FindByIdAsync(timeOffRequestId);
            if (request == null)
            {
                return NotFound();
            }

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var approval = await _approvalService.FindByUserIdAndRequestId(userId, timeOffRequestId);
            if (approval == null)
            {
                return Forbid();
            }

            await _approvalService.ApproveAsync(approval);

            return NoContent();
        }

        [HttpPut("Reject/{timeOffRequestId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Reject(Guid timeOffRequestId)
        {
            TimeOffRequest request = await _timeOffRequestService.FindByIdAsync(timeOffRequestId);
            if (request == null)
            {
                return NotFound();
            }

            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var approval = await _approvalService.FindByUserIdAndRequestId(userId, timeOffRequestId);
            if (approval == null)
            {
                return Forbid();
            }

            await _approvalService.RejectAsync(approval);

            return NoContent();
        }
    }
}
