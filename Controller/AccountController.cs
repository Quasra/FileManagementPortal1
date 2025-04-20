using FileManagementPortal1.DTOs.Account;
using FileManagementPortal1.Models;
using FileManagementPortal1.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace FileManagementPortal1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly Func<AppUser, Task<string>> _generateJwtToken;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            UserRepository userRepository,
            Func<AppUser, Task<string>> generateJwtToken,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _generateJwtToken = generateJwtToken;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return BadRequest("Email is already taken");

            if (await _userManager.FindByNameAsync(registerDto.UserName) != null)
                return BadRequest("Username is already taken");

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userRepository.CreateUserAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userRepository.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = await _generateJwtToken(user),
                Roles = new[] { "User" }
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.UserName);

            if (user == null) return Unauthorized("Invalid username or password");

            if (!user.IsActive) return Unauthorized("Account is deactivated");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid username or password");

            var roles = await _userRepository.GetUserRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = await _generateJwtToken(user),
                Roles = roles.ToArray()
            };
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.Identity.Name);

            if (user == null) return NotFound();

            var roles = await _userRepository.GetUserRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = await _generateJwtToken(user),
                Roles = roles.ToArray()
            };
        }
    }
}