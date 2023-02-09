using FinancialApi.Data;
using FinancialApi.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
            else return BadRequest($"Username {user.Username} already exist");

            return Ok($"User {user.Username} is added!");
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
                return BadRequest("User not found.");
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password!");
            }
            string token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.Username),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

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
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }



    }
}
