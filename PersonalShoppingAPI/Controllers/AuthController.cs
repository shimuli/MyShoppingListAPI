using AuthenticationPlugin;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Repository.IRepo;
using PersonalShoppingAPI.Utills;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthService _auth;
        Random random = new();
        string _baseUrl;
        private readonly IConfiguration _configuration;
        public AuthController(SHOPPINGLISTContext context, IMapper mapper, IUserRepo iUserRepo, 
            IHttpContextAccessor httpContext, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _iUserRepo = iUserRepo;
            var request = httpContext.HttpContext.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}";
            _webHostEnvironment = webHostEnvironment;

            // for jwt
            _configuration = configuration;
            _auth = new AuthService(_configuration);

        }      

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] CreateAccountDto createAccountDto )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                if(createAccountDto.PhoneNumber.Length >10 || createAccountDto.PhoneNumber[..1] !="0" ) // str.Substring(0, 1);
                {
                    return BadRequest(new { message = "Invalid phone number, must start with 0 and must be 10 digits" });
                }

                var users = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == createAccountDto.PhoneNumber);
                if(users != null)
                {
                    return Unauthorized(new {message = "Phone Number is taken"});
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


                string randomNum = Convert.ToString(random.Next(1000, 9999));
                createAccountDto.Password = SecurePasswordHasherHelper.Hash(createAccountDto.Password);

                var adduser = _mapper.Map<User>(createAccountDto);
                adduser.Role = "user";
                adduser.IsActive = true;
                adduser.IsVerified = false;
                adduser.VerificationCode = int.Parse(randomNum);
                adduser.DateCreated = System.DateTime.Now;
                adduser.DateUpdated = System.DateTime.Now;
                adduser.ImageUrl = imageUrl;

             

                await _context.Users.AddAsync(adduser);
                await _context.SaveChangesAsync();

                // verify sms
                string message = $"Your verification code is {adduser.VerificationCode}";
                var systemdefaults = await _context.Systemdefaults.FirstOrDefaultAsync();
                var response = SmsService.VerifyAccount(systemdefaults.SmsuserId, systemdefaults.Smskey, adduser.PhoneNumber, message);

                if(response == "Sent")
                {
                    return StatusCode(StatusCodes.Status201Created, adduser);
                }
                else
                {
                    return BadRequest(new { smserror = response });
                }
                

            }
            catch (System.Exception ex)
            {
                return BadRequest(new { exception = ex.Message });
            }

            
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login( AuthDto loginDto)
        {
            try
            {
                var authuser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber);

                if (authuser == null || !SecurePasswordHasherHelper.Verify(loginDto.Password, authuser.Password))
                {
                    return Unauthorized(new { message = "Invalid phone number or password" });
                }

                _mapper.Map<User>(loginDto);
                var claims = new[]
                {
                //new Claim(JwtRegisteredClaimNames.Sub, _configuration["Tokens:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim(ClaimTypes.Role, authuser.Role),
                //new Claim(JwtRegisteredClaimNames.Exp, expTime.ToString()),
                new Claim("id", authuser.Id.ToString()),
                new Claim("username", authuser.FullName),
                new Claim("verified", authuser.IsVerified.ToString()),
                new Claim("isactive", authuser.IsActive.ToString()),

             };
                var token = _auth.GenerateAccessToken(claims);

                return new ObjectResult(new
                {
                    token.AccessToken,
                    token.TokenType,
                    token.ExpiresIn,
                    authuser.Id,
                    authuser.FullName,
                    authuser.PhoneNumber,
                    authuser.ImageUrl,
                    authuser.IsVerified,

                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {message =ex.Message});
            }
           
        }


        [HttpPost("VerfiyPhone")]
        public async Task<IActionResult> VerfiyPhone(VerifyPhoneDto verifyDto)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == verifyDto.PhoneNumber); // int.Parse(User.FindFirstValue("id"))

                if (getUser == null || verifyDto.PhoneNumber != getUser.PhoneNumber)
                {
                    return NotFound(new { message = "Phone number does not exist" });
                }
                if (getUser.IsVerified == true)
                {
                    return BadRequest(new { message = "user is already verified" });
                }


                if (verifyDto.VerificationCode != getUser.VerificationCode)
                {
                    return Unauthorized(new { messagge = "Invalid code" });
                }

                getUser.IsVerified = true;
                getUser.VerificationCode = null;
                //_context.Users.Update(info);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User was verified succesfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
           
        }


        [HttpGet("ResendCode")]
        public async Task<IActionResult> ResendCode(string phonenumber)
        {
            try
            {
                // string userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //var user = await _moviesDbContext.Users.Where(a => a.Id == userId).Include(a => a.Songs).ToListAsync();

                var userPhone = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phonenumber);
                if (userPhone == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                if (userPhone.IsVerified == true)
                {
                    //return NotFound(new JsonResult(notVerified) { StatusCode = 403 });
                    return BadRequest(new { message = "This user was verified" });
                }

                // verify sms
                string message = $"Your verification code is {userPhone.VerificationCode}";
                var systemdefaults = await _context.Systemdefaults.FirstOrDefaultAsync();
                var response = SmsService.VerifyAccount(systemdefaults.SmsuserId, systemdefaults.Smskey, userPhone.PhoneNumber, message);

                if (response == "Sent")
                {
                    return new ObjectResult(new
                    {
                        message = "Code was sent succesfully",
                        userId = userPhone.Id,
                        code = userPhone.VerificationCode
                    });
                }
                else
                {
                    return BadRequest(new { smserror = response });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        

        }


        [HttpPost("{id}")]
        public async Task<IActionResult> VerfiyPhone(int id, [FromForm] VerifyPhoneDto verifyDto)
        {
            _logger.LogInformation(verifyDto.Phone + " verified account");
            var getUser = await _moviesDbContext.Users.FindAsync(id);

            if (getUser == null)
            {
                return NotFound();
            }

            var userPhone = await _moviesDbContext.Users.FirstOrDefaultAsync(u => u.Phone == verifyDto.Phone);
            if (userPhone == null)
            {
                return NotFound();
            }
            if (userPhone.confirmCode == null)
            {
                //return StatusCode(StatusCodes.Status406NotAcceptable);
                return new ObjectResult(new
                {
                    message = "This number was verified"
                });

            }

            if (verifyDto.confirmCode != userPhone.confirmCode)
            {
                return Unauthorized();
            }



            _mapper.Map<User>(verifyDto);

            getUser.PhoneVerified = true;
            getUser.confirmCode = null;
            await _moviesDbContext.SaveChangesAsync();

            return new ObjectResult(new
            {
                message = "Phone number verified successfully",
                verifiedPhone = userPhone.PhoneVerified,
            });
        }

    }
}
