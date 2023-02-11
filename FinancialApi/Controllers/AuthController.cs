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
    public class AuthController : ControllerBase, IAuth
    {
        readonly FinancialDbContext context;
        private readonly IConfiguration configuration;

        public AuthController(FinancialDbContext _context, IConfiguration configuration)
        {
            context = _context;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            if (context.Users.FirstOrDefault(u => u.Username == user.Username) == null)
            {
                context.Users.Add(user);
                context.SaveChanges();
            }
            else return BadRequest(new JsonStatusResult { message = $"Username {user.Username} already exist" });

            return Ok(new JsonStatusResult { message = $"User {user.Username} is added!" });
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var user = context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new JsonStatusResult { message = "User not found" });
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new JsonStatusResult { message = "Wrong password" });
            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(user, refreshToken);

            return Ok(token);
        }

        [HttpPost("refresh-token"), Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var user = context.Users.FirstOrDefault(u => u.Username.Equals(User.Identity.Name));
            if (user == null)
            {
                return Unauthorized(new JsonStatusResult { message = "User is not authorized" });
            }
            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized(new JsonStatusResult { message = "Refresh token is invalid" });
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized(new JsonStatusResult { message = "Refresh token is expired" });
            }
            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(Response.Cookies, user, newRefreshToken);

            return Ok(newRefreshToken);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            return refreshToken;
        }

        private void SetRefreshToken(IResponseCookies cookies, User user, RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;

            context.SaveChanges();
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.Gender,"femaale")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.
                GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public Task<ActionResult<string>> Logout()
        {
            throw new NotImplementedException();
        }
    }
}
