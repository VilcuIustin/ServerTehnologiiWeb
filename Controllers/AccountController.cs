using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server_Tehnologii_Web.Models;
using Server_Tehnologii_Web.Payloads;

namespace Server_Tehnologii_Web.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ServerContext _db;
        private IConfiguration Config { get; }
        public AccountController(ServerContext dbContext, IConfiguration config)
        {
            this._db = dbContext;
            this.Config = config;
        }
        
          

        [Route("register")]
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] LoginPayload payload)
        {
            try
            {
                var m = new MailAddress(payload.Email);
            }
            catch (Exception)
            {
                return new JsonResult(new {status = false, message = "email format " + payload.Email});
            }

            try
            {
                var existingUserWithMail = _db.Users
                    .Any(u => u.Email == payload.Email);

                if (existingUserWithMail)
                    return new JsonResult(new {status = false, message = "An account with this email already exists "});
                
                var userToCreate = new User
                {
                    Email = payload.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(payload.Password),
                    Nume = payload.Nume
                };

                await _db.Users.AddAsync(userToCreate);

                await _db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return new JsonResult(new {error = ex.Message});
            }
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginPayload loginPayload)
        {
            try
            {
                var m = new MailAddress(loginPayload.Email);
            }
            catch (FormatException)
            {
                Response.StatusCode = 400;
                return new JsonResult(new {status = "false", message = "email format"});
            }

            var foundUser = _db.Users
                .SingleOrDefault(u => u.Email == loginPayload.Email);

            if (foundUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginPayload.Password, foundUser.Password))
                {
                    var tokenString = GenerateJsonWebToken(foundUser);

                    return new JsonResult(new
                    {
                        foundUser.Email,
                        foundUser.Id,
                        foundUser.Nume,
                        token = tokenString
                    });
                }

                return BadRequest(new {status = false, message = "Wrong password or email "});
            }

            return BadRequest(new {status = false, message = "No user with this email found"});
        }


        private string GenerateJsonWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("Id", user.Id.ToString()),
                new Claim("Nume", user.Nume),
                new Claim("Role", "user")
            };

            var token = new JwtSecurityToken(Config["Jwt:Issuer"],
                Config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    
        
        
    }
}