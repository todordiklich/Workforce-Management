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
using WFM.Models.Requests.UserRequests;
using WFM.Models.DTO.Responses.UserResponses;

namespace WFM.WEB.Controllers
{
    [ApiController]
    [Authorize(Roles = GlobalConstants.AdminRoleName)]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseDTO>> Get(Guid id)
        {
            User userFromDB = await _userService.GetByIdAsync(id);

            if (userFromDB == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<UserResponseDTO>(userFromDB));
        }

        [HttpGet("All")]
        [ProducesResponseType(typeof(List<UserResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseDTO>> GetAll()
        {
            var usersFromDB = await _userService.GetAllAsync();

            return Ok(_mapper.Map<IEnumerable<UserResponseDTO>>(usersFromDB));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(UserCreateRequestDTO userDTO)
        {
            User user = _mapper.Map<User>(userDTO);

            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            bool isCreated = await _userService.CreateAsync(user, userDTO.Password, userDTO.RoleName, currentUserId);
            User userFromDb = await _userService.GetByNameAsync(userDTO.UserName);
            if (isCreated)
            {
                bool isAdded = await _userService.AddUserToRole(userFromDb, userDTO.RoleName);
                if (!isAdded)
                {
                    return BadRequest();
                }

                return CreatedAtAction("Get", "Users", new { id = userFromDb.Id }, userDTO);
            }

            return BadRequest();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(UserEditRequestDTO userUpdateDTO)
        {
            var user = await _userService.GetByIdAsync(userUpdateDTO.Id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userService.CheckIfRoleExists(userUpdateDTO.RoleName) || !await _userService.CheckIfCountryOfResidenceExists(userUpdateDTO.CountryOfResidence))
            {
                return BadRequest();
            }

            _mapper.Map(userUpdateDTO, user, typeof(UserEditRequestDTO), typeof(User));

            await _userService.AddUserToRole(user, userUpdateDTO.RoleName);
            
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _userService.UpdateAsync(user, currentUserId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            User user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            bool isDeleted = await _userService.DeleteAsync(user);
            if (!isDeleted)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
