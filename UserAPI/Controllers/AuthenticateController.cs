//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using Microsoft.AspNetCore.Authorization;
//using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using UserAPI.Models;
//using System.Text;
//using System.Security.Claims;
using UserAPI.Utility;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
//using System.Numerics;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        //private IConfiguration _config;
        private readonly UserAPIContext _context;
        private readonly IntSession _session;

        public AuthenticateController(UserAPIContext context, IntSession session)
        {
            //_config = config;
            _context = context;
            _session = session;
            _session.context = _context;
            //_session.config = _config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            IActionResult response = Unauthorized();

            /*var user = _session.Authenticate(login);

            if (user != null)
            {
                var tokenString = _session.GenerateToken(user);

                //[pqa] Start session
                AccessToken token = null;
                token = new AccessToken { UserName = user.UserName, TokenString = tokenString };
                _session.Start(token);

                response = Ok(new { username=user.UserName, userType = user.UserType, email = user.Email, token = tokenString });
            }*/

            //[pqa] Start the session
            AccessToken resultingToken = _session.Start(login);
                
            if (resultingToken != null)
            {
                //response = Ok(new { username = resultingToken.UserName, userType = resultingToken.UserType, email = resultingToken.Email, token = resultingToken.TokenString });
                response = Ok(new { resultingToken });
            }

            return response;
        }

        [HttpPost("Logout")]
        [Authorize]
        public ActionResult Logout()
        {

            _session.Close(User.Identity.Name);
            return Ok();
        }

        [HttpPost("RefreshToken")]
        //[Authorize]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        //public ActionResult RefreshToken()
        {
            //var userName = User.Identity.Name;
            try
            {
                //_logger.LogInformation($"User [{userName}] is trying to refresh JWT token.");

                /*if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }*/

                //var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                AccessToken jwtResult = _session.RefreshToken(request.AccessToken, request.RefreshToken);
                //_logger.LogInformation($"User [{userName}] has refreshed JWT token.");
                return Ok(new { jwtResult });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); // return 401 so that the client side can redirect the user to login page
            }

            //return NoContent();
        }

        /*private AuthModel Authenticate(LoginRequest login)
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
        }*/

        /*private string GenerateToken(AuthModel user)
        {
            //[pqa] generate the token
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
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:accessTokenValidity"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }*/
    }
}
