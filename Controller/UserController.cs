using FileManagementPortal1.DTOs.Account;
using FileManagementPortal1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;

namespace FileManagementPortal1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public UserController(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "Current user not found" });

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (id != userUpdateDto.Id)
                return BadRequest(new { message = "User IDs do not match" });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            // Update user properties
            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.Username;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to update user", errors = result.Errors });

            var updatedUserDto = _mapper.Map<UserDto>(user);
            return Ok(updatedUserDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user", errors = result.Errors });

            return Ok(new { message = $"User {user.UserName} deleted successfully" });
        }

        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddToRole(string id, [FromBody] AddToRoleDto addToRoleDto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var roleExists = await _roleManager.RoleExistsAsync(addToRoleDto.RoleName);

            if (!roleExists)
                return BadRequest(new { message = $"Role {addToRoleDto.RoleName} does not exist" });

            var result = await _userManager.AddToRoleAsync(user, addToRoleDto.RoleName);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to add user to role", errors = result.Errors });

            return Ok(new { message = $"Rol Eklendi. {user.UserName} -->  {addToRoleDto.RoleName}" });
        }

        [HttpDelete("{id}/roles/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFromRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var result = await _userManager.RemoveFromRoleAsync(user, role);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to remove user from role", errors = result.Errors });

            return Ok(new { message = $"Removed {user.UserName} from role {role}" });
        }
    }
}