﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using UserAPI.Models;
using UserAPI.Utility;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserAPIContext _context;
        private readonly IntSession _session;

        public UsersController(UserAPIContext context, IntSession session)
        {
            _context = context;
            _session = session;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        {
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            //[pqa] Prepare the query that will not return deleted users.
            IQueryable<UserModel> query = _context.Users.Where(e => e.Status != UserStatus.Deleted);

            //[pqa] Return only the user's with usertype "User"
            if (User.IsInRole(Policy.User))
            {
                query = query.Where(e => e.UserType == UserTypes.User);
                /*return await _context.Users
                    .Where(e => e.UserType == UserTypes.User)
                    .Where(e => e.Status != 0)
                    .ToListAsync();*/
            }

            //return await _context.Users.ToListAsync();
            return await query.ToListAsync();
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet("{id}")]
        //public async Task<ActionResult<UserModel>> GetUserModel(long id)
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUserModel(long id)
        {
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            //[pqa] Prepare the query that will not return deleted users.
            IQueryable<UserModel> query = _context.Users
                .Where(e => e.Status != UserStatus.Deleted)
                .Where(e => e.Id == id); 

            //[pqa] Return only the user's with usertype is "User"
            if (User.IsInRole(Policy.User))
            {
                query = query.Where(e => e.UserType == UserTypes.User);
            }

            //[pqa] Put the result in the variable so that we can check it if is returning anything
            var userModel = await query.ToListAsync();

            if (userModel.Count()== 0)
            {
                return NotFound();
            }

            return userModel;
        }

        // GET: api/Users/UserTypes/Admin
        [Authorize]
        [HttpGet("UserTypes/{userType}")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetByUserType(UserTypes userType)
        {
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            //[pqa] Prepare the query that will not return deleted users.
            IQueryable<UserModel> query = _context.Users.Where(e => e.Status != UserStatus.Deleted);

            //[pqa] Return only the user's if usertype is "User"
            if (User.IsInRole(Policy.User))
            {
                if (userType == UserTypes.User) {
                    query = query.Where(e => e.UserType == userType);
                }
                else
                {
                    //[pqa] If the user type is "User" and the search is not "User" it should not return anything.
                    return Unauthorized();
                }
            }
            else 
            {
                query = query.Where(e => e.UserType == userType);
            }

            //[pqa] Put the result in the variable so that we can check it if is returning anything
            var userModel = await query.ToListAsync();

            if (userModel.Count() == 0)
            {
                return NotFound();
            }

            return userModel;
        }

        // GET: api/Users/UserTypes/Admin
        [Authorize]
        [HttpGet("Search/{searchKey}")]
        public async Task<ActionResult<IEnumerable<UserModel>>> Search(string searchKey, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            //int _pageNumber = pageNumber.HasValue ? pageNumber.Value : 0;
            //int _pageSize = pageSize.HasValue ? pageSize.Value : 0;

            //[pqa] Prepare the query that will not return deleted users.
            IQueryable <UserModel> query = _context.Users.Where(e => e.Status != UserStatus.Deleted);

            query = query.Where(u => u.FirstName.Contains(searchKey) || u.LastName.Contains(searchKey) || u.Email.Contains(searchKey));

            //[pqa] Return only the user's if usertype is "User"
            if (User.IsInRole(Policy.User))
            {
                query = query.Where(e => e.UserType == UserTypes.User);
            }

            //[pqa] Pagination thingy.
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = Paginate.PagedQuery(pageNumber.Value, pageSize.Value, query);
            }

            //[pqa] Put the result in the variable so that we can check it if is returning anything
            var userModel = await query.ToListAsync();

            if (userModel.Count() == 0)
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
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

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
            //[pqa] TO check if session is valid. This is to enable logout.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            //[pqa] Encrypt password and add the created time/by details.
            userModel.Password = Cipher.Encrypt(userModel.Password, userModel.Email);
            userModel.CreatedTime = DateTime.Now;
            userModel.CreatedBy = User.Identity.Name; 

            _context.Users.Add(userModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserModel", new { id = userModel.Id }, userModel);
        }

        // DELETE: api/Users/5
        [Authorize(Policy = Policy.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserModel(long id)
        {
            //[pqa] TO check if session is valid. This is to enable logout and invalidate the old token.
            if (!_session.isSessionValid(User.Identity.Name, HttpContext.Request))
            {
                return Unauthorized();
            }

            var userModel = await _context.Users.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }

            //[pqa] Soft delete. Just set status to zero.
            userModel.Status = UserStatus.Deleted;

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
