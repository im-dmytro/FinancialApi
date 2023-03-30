using FinancialApi.Data;
using FinancialApi.Models;
using FinancialApi.Models.Account;
using FinancialApi.Models.Auth;
using FinancialApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinancialApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly FinancialDbContext context;
        private readonly IJwtAuthService authService;

        public AuthController(FinancialDbContext _context, IJwtAuthService _authService)
        {
            context = _context;
            authService = _authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserDto request)
        {
            if (ModelState.IsValid)
            {
                authService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                };

                if (context.Users.FirstOrDefault(u => u.Username == user.Username) == null)
                {
                    context.Users.Add(user);
                    context.SaveChanges();
                }
                else return BadRequest(new JsonStatusResult { message = $"Username {user.Username} already exist" });

                return StatusCode(201);
            }
            else { return BadRequest(new JsonStatusResult { message = "ModelState is not right" }); }

        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLogin request)
        {
            var user = context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new JsonStatusResult { message = "User not found" });
            }
            if (!authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new JsonStatusResult { message = "Wrong password" });
            }

            string token = authService.CreateToken(user);

            var refreshToken = authService.GenerateRefreshToken();
            authService.SetRefreshToken(Response.Cookies, user, refreshToken);

            return Ok(new { access_token = token });
        }

        [HttpGet("get-name"), Authorize]
        public async Task<ActionResult<string>> GetUserName()
        {
            return User.Identity.Name;
        }

        [HttpGet("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var user = context.Users.FirstOrDefault(u => u.RefreshToken.Equals(refreshToken));
            if (user == null)
            {
                return Unauthorized(new JsonStatusResult { message = "User is not authorized" });
            }
            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Forbid();
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized(new JsonStatusResult { message = "Refresh token is expired" });
            }
            string token = authService.CreateToken(user);
            var newRefreshToken = authService.GenerateRefreshToken();
            authService.SetRefreshToken(Response.Cookies, user, newRefreshToken);

            return Ok(new { access_token = token });
        }

    }
}
