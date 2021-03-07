using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAPI.Models;
using UserAPI.Utility;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserAPIContext _context;

        public UsersController(UserAPIContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        {
            //[pqa] Return only the user's with usertype "User"
            if (User.IsInRole(Policy.User))
            {
                return await _context.Users.Where(e => e.UserType == UserTypes.User).ToListAsync();
            }
                
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserModel(long id)
        {
            var userModel = await _context.Users.FindAsync(id);

            if (userModel == null)
            {
                return NotFound();
            }

            return userModel;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserModel(long id, [FromBody] UserModelUpdate userModelUpdate)
        {   
            /*if (id != userModelUpdate.Id)
            {
                return BadRequest();
            }*/

            //[pqa] The id to update should be in the database.
            var userModel = await _context.Users.FindAsync(id);
            if (userModel==null)
            {
                return NotFound();
            }

            //[pqa] Make sure that the user can only update own data.
            if (User.IsInRole(Policy.User))
            {
                if (User.Identity.Name != userModel.UserName)
                {
                    return Unauthorized();
                }
            }
            
            //[pqa] Make sure that only the changed fields are impacted.
            userModel.UserName = userModelUpdate.UserName is null ? userModel.UserName : userModelUpdate.UserName;
            userModel.Email = userModelUpdate.Email is null ? userModel.Email : userModelUpdate.Email;
            userModel.Password = userModelUpdate.Password is null ? userModel.Password : Cipher.Encrypt(userModelUpdate.Password, userModel.Email);
            userModel.FirstName = userModelUpdate.FirstName is null ? userModel.FirstName : userModelUpdate.FirstName;
            userModel.LastName = userModelUpdate.LastName is null ? userModel.LastName : userModelUpdate.LastName;
            userModel.UserType = userModelUpdate.UserType == 0 ? userModel.UserType : userModelUpdate.UserType;
            userModel.Status = userModelUpdate.Status == 0 ? userModel.Status : userModelUpdate.Status;
            userModel.UpdatedTime = DateTime.Now;
            userModel.UpdatedBy = User.Identity.Name;
            _context.Entry(userModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Policy = Policy.Admin)]
        [HttpPost]
        public async Task<ActionResult<UserModel>> PostUserModel(UserModel userModel)
        {
            //var currentUser = HttpContext.User;
            //currentUser.Identity.
            userModel.Password = Cipher.Encrypt(userModel.Password, userModel.Email);
            userModel.CreatedTime = DateTime.Now;
            userModel.CreatedBy = User.Identity.Name; //currentUser.Identity.Name; //currentUser.Claims.FirstOrDefault(e=>e.Type=="Sub").Value;
            _context.Users.Add(userModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserModel", new { id = userModel.Id }, userModel);
        }

        // DELETE: api/Users/5
        [Authorize(Policy = Policy.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserModel(long id)
        {
            var userModel = await _context.Users.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }

            userModel.Status = 0;

            //_context.Users.Remove(userModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserModelExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
