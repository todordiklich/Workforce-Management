using System;
using AutoMapper;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

using WFM.Common;
using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.Models.Requests.TeamRequests;
using WFM.Models.DTO.Responses.TeamResponses;

namespace WFM.WEB.Controllers
{
    [Route("api/[controller]/[action]/")]
    [ApiController]
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IMapper _mapper;

        public TeamsController(ITeamService teamService, IMapper mapper)
        {
            _teamService = teamService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrRegularRoleName")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TeamResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<TeamResponseDTO>> Get(Guid id)
        {
            Team teamFromDB = await _teamService.GetByIdAsync(id);

            if (teamFromDB == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<TeamResponseDTO>(teamFromDB));
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrRegularRoleName")]
        [ProducesResponseType(typeof(List<TeamResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<TeamResponseDTO>> GetAll()
        {
            var teamsFromDB = await _teamService.GetAllAsync();

            return Ok(_mapper.Map<IEnumerable<TeamResponseDTO>>(teamsFromDB));
        }

        [HttpPost]
        [Authorize(Roles = GlobalConstants.AdminRoleName)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TeamBaseRequestDTO teamCreateRequestDTO)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Team team = _mapper.Map<Team>(teamCreateRequestDTO);

            Team teamFromDb = await _teamService.CreateAsync(team, currentUserId);
            if(teamFromDb == null)
            {
                return BadRequest();
            }

            return CreatedAtAction("Get", "Teams", new { id = teamFromDb.Id }, teamCreateRequestDTO);
        }

        [HttpPut("{teamId}")]
        [Authorize(Roles = GlobalConstants.AdminRoleName)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(Guid teamId, TeamBaseRequestDTO teamBaseRequestDTO)
        {
            Team team = await _teamService.GetByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            _mapper.Map(teamBaseRequestDTO, team, typeof(TeamBaseRequestDTO), typeof(Team));

            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Team teamFromDb = await _teamService.UpdateAsync(team, currentUserId);

            if(teamFromDb == null)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("{teamId}")]
        [Authorize(Roles = GlobalConstants.AdminRoleName)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid teamId)
        {
            Team team = await _teamService.GetByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Team teamFromDb = await _teamService.DeleteAsync(team, currentUserId);

            if (teamFromDb == null)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPut]
        [Authorize(Roles = GlobalConstants.AdminRoleName)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignUserToATeam(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO)
        {
            Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Team teamFromDb = await _teamService.AddMemberAsync(teamAddRemoveMemberDTO, currentUserId);

            if(teamFromDb == null)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPut]
        [Authorize(Roles = GlobalConstants.AdminRoleName)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveUserFromATeam(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO)
        {
            Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Team teamFromDb = await _teamService.RemoveMemberAsync(teamAddRemoveMemberDTO, currentUserId);

            if (teamFromDb == null)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
