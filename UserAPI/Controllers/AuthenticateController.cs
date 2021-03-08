using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using UserAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using UserAPI.Utility;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private IConfiguration _config;
        private readonly UserAPIContext _context;

        public AuthenticateController(IConfiguration config, UserAPIContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            IActionResult response = Unauthorized();

            var user = Authenticate(login);

            if (user != null)
            {
                var tokenString = GenerateToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        } 

        private AuthModel Authenticate(LoginRequest login)
        {
            string defUserName = "";
            string defPassword = "";

            AuthModel user = null;

            //[pqa] Check if database is not empty.
            if (_context.Users.Count()>0)
            {
                //[pqa] If database is NOT empty, evaluate the username and password against the value in the database.
                var reqUser = _context.Users.SingleOrDefault(e => e.UserName == login.UserName);
                if (reqUser!=null) { 
                    if (reqUser.Status!=0) { 
                        if (Cipher.Decrypt(reqUser.Password, reqUser.Email) == login.Password)
                        {
                            user = new AuthModel { UserName = login.UserName, Email = reqUser.Email, UserType = reqUser.UserType, Status = reqUser.Status };
                        }
                    }
                }
            } 
            else
            {
                //[pqa] If database is empty, default the password.
                defUserName = "Admin";
                defPassword = "Password1";
                if (login.UserName == defUserName && login.Password == defPassword)
                {
                    user = new AuthModel { UserName = "Admin", Email = "admin@test.com", UserType = UserTypes.Admin, Status = UserStatus.Active };
                }
            }

            return user;
        }

        private string GenerateToken(AuthModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.UserType.ToString()),
                new Claim("status", user.Status.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
