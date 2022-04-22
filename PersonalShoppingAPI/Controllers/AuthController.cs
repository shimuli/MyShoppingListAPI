using AuthenticationPlugin;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Repository.IRepo;
using System;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly IUserRepo _iUserRepo;
        Random random = new();
        public AuthController(SHOPPINGLISTContext context, IMapper mapper, IUserRepo iUserRepo)
        {
            _context = context;
            _mapper = mapper;
            _iUserRepo = iUserRepo;

        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers(string username)
        {
            try
            {
                var users = await _iUserRepo.GetUsersAsync();
                return Ok(users);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new {exception = ex.Message});
            }
            
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount(CreateAccountDto createAccountDto )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var users = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == createAccountDto.PhoneNumber);
                if(users != null)
                {
                    return Unauthorized(new {message = "Phone Number is taken"});
                }
                string randomNum = Convert.ToString(random.Next(1000, 9999));

                createAccountDto.Role = "user";
                createAccountDto.IsActive = true;
                createAccountDto.IsVerified = false;
                createAccountDto.VerificationCode = int.Parse(randomNum);
                createAccountDto.DateCreated = System.DateTime.Now;
                createAccountDto.DateUpdated = System.DateTime.Now;
                createAccountDto.ImageUrl = "hhfdfhgdhgfhdgdfd";
                createAccountDto.Password = SecurePasswordHasherHelper.Hash(createAccountDto.Password);


            }
            catch (System.Exception ex)
            {
                return BadRequest(new { exception = ex.Message });
            }

            
        }

    }
}

//public string ImageUrl { get; set; }


//[JsonIgnore]
//public string Role { get; set; }

//[JsonIgnore]
//public bool IsActive { get; set; }
//[JsonIgnore]
//public DateTime? DateCreated { get; set; }
//[JsonIgnore]
//public DateTime? DateUpdated { get; set; }

//[JsonIgnore]
//public int? VerificationCode { get; set; }

//[JsonIgnore]
//public bool? IsVerified { get; set; }