using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Repository.IRepo;
using System;
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
                return Ok(users);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { exception = ex.Message });
            }

        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateAdminDto createAdminDto)
        {

        }
    }
}
