using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.User;
using financing_api.Services.AuthService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using financing_api.Data;
using financing_api.Dtos.User;
using Microsoft.Extensions.Options;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtGenerator _jwtGenerator;

        public AuthController(
            IAuthService authService,
            IConfiguration configuration,
            IOptionsSnapshot<UserSettings> options
        )
        {
            _authService = authService;
            _jwtGenerator = new JwtGenerator(configuration["JwtPrivateSigningKey"]);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<LoadUserDto>>> Register(UserRegisterDto request)
        {
            var response = await _authService.Register(
                new User
                {
                    FirstName = request.Firstname,
                    LastName = request.Lastname,
                    Email = request.Email
                },
                request.Password
            );

            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<LoadUserDto>>> Login(UserLoginDto request)
        {
            var response = await _authService.Login(request.Email, request.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Load current user
        [Authorize]
        [HttpGet("load-user")]
        public async Task<ActionResult<ServiceResponse<LoadUserDto>>> LoadUser()
        {
            var response = await _authService.LoadUser();

            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }

        // Google login
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] string idToken)
        {
            GoogleJsonWebSignature.ValidationSettings settings =
                new GoogleJsonWebSignature.ValidationSettings();

            // Change this to your google client ID
            settings.Audience = new List<string>()
            {
                "422382677454-ugb68rv18u7a9hc3or7qfe12dmuif1pu.apps.googleusercontent.com"
            };

            GoogleJsonWebSignature.Payload payload = GoogleJsonWebSignature
                .ValidateAsync(idToken, settings)
                .Result;
            return Ok(new { AuthToken = _jwtGenerator.CreateUserAuthToken(payload.Email) });
        }

        [Authorize]
        [HttpDelete("user")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteUser()
        {
            var response = await _authService.DeleteUser();

            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
    }
}
