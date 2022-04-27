using AuthenticationPlugin;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Repository.IRepo;
using PersonalShoppingAPI.Utills;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly IUserRepo _iUserRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        Random random = new();
        string _baseUrl;
        public UsersController(SHOPPINGLISTContext context, IMapper mapper, IUserRepo iUserRepo, IHttpContextAccessor httpContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _iUserRepo = iUserRepo;
            var request = httpContext.HttpContext.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}";
            _webHostEnvironment = webHostEnvironment;

        }
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers(string username)
        {
            try
            {
                var users = string.IsNullOrEmpty(username) ? await _context.Users.ToListAsync()
                    : await _context.Users.Where(c => c.FullName.Contains(username)).ToListAsync();
                if (users.Count > 0)
                {
                    return Ok(users);
                }
                else
                {
                    return NotFound(new { message = "No data to display" });
                }
                
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { exception = ex.Message });
            }

        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromForm] CreateAdminDto createAdminDto)
        {
            try
            {
                var users = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == createAdminDto.PhoneNumber);
                if (users != null)
                {
                    return Unauthorized(new { message = "Phone Number exists" });
                }
                // get image
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string imageUrl = String.Empty;
                if (files.Count > 0)
                {
                    string upload = webRootPath + WebContants.ProfileImages;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }

                    imageUrl = _baseUrl + WebContants.ProfileImages + fileName + extension;
                }
                else
                {
                    imageUrl = null;
                }
                createAdminDto.Password = SecurePasswordHasherHelper.Hash(createAdminDto.Password);

                var adduser = _mapper.Map<User>(createAdminDto);
                adduser.Role = "Admin";
                adduser.IsActive = true;
                adduser.IsVerified = true;
                adduser.VerificationCode = null;
                adduser.DateCreated = System.DateTime.Now;
                adduser.DateUpdated = System.DateTime.Now;
                adduser.ImageUrl = imageUrl;
                await _context.Users.AddAsync(adduser);
                await _context.SaveChangesAsync();

                return Ok(adduser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUser(int Id)
        {
            try
            {
                var users = await _context.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (users == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                users.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new {message = $"{users.FullName} is now blocked"});
            }
            catch (Exception ex)
            {
                return BadRequest(new {message=ex.Message});
            }

        }


        [HttpPost("ActivateUser")]
        public async Task<IActionResult> ActivateUser(int Id)
        {
            try
            {
                var users = await _context.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (users == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                users.IsActive = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"{users.FullName} is now active" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                // remove user image
                if (user.ImageUrl != null)
                {
                    string webRootpath = _webHostEnvironment.WebRootPath;
                    string uploadx = webRootpath + WebContants.ProfileImages;
                    var oldFile = Path.Combine(uploadx, Path.GetFileName(user.ImageUrl));
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                _context.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
            
        }

    }
}
