using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserAPI.Models;
using UserAPI.Utility;
using Microsoft.IdentityModel.Tokens;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserAPIContext _context;
        private readonly IntSession _session;

        public AuthenticateController(UserAPIContext context, IntSession session)
        {
            //[pqa] Initialization
            _context = context;
            _session = session;
            _session.context = _context;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            IActionResult response = Unauthorized(_session.ResponseMsg("Unauthorized","Invalid login credential."));

            //[pqa] Start the session
            AccessToken resultingToken = _session.Start(login);
                
            if (resultingToken != null)
            {   
                response = Ok(new { resultingToken });
            }

            return response;
        }

        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            IActionResult response = Unauthorized(_session.ResponseMsg("Unauthorized", "Not authorized to logout."));

            //[pqa] Simply close the session
            if (_session.Close(User.Identity.Name))
            {
                response = Ok(_session.ResponseMsg("Success", "The user has been logged out."));
            }

            return response;
        }

        [HttpPost("RefreshToken")]
        //[Authorize]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                AccessToken jwtResult = _session.RefreshToken(request.AccessToken, request.RefreshToken);
                return Ok(new { jwtResult });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(_session.ResponseMsg("Error", e.Message)); 
            }
        }
    }
}
