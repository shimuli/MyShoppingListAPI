using AuthenticationPlugin;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                if(authuser.IsActive == false)
                {
                    return Unauthorized(new { message = "The user is blocked" });
                }
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
                 new Claim("phoneNumber", authuser.PhoneNumber),
                new Claim("verified", authuser.IsVerified.ToString()),
                new Claim("isactive", authuser.IsActive.ToString()),

             };
                var token = _auth.GenerateAccessToken(claims);

                LoginTimeUpdate(authuser);
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


        [HttpGet("ForgotPasswordVerifyCode")]
        public async Task<IActionResult> ForgotPasswordVerifyCode(string phoneNumber)
        {
            var getUser = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (getUser == null || getUser.PhoneNumber != phoneNumber)
            {
                return NotFound(new { message = "invalid user phone number" });
            }

            if (getUser.IsVerified == false)
            {
                return BadRequest(new { message = "User is not verified" });
            }

            if(getUser.ForgotPasswordCode != null)
            {
                // verify sms
                string message = $"Your verification code is {getUser.ForgotPasswordCode}";
                var systemdefaults = await _context.Systemdefaults.FirstOrDefaultAsync();
                var smsResponse = SmsService.VerifyAccount(systemdefaults.SmsuserId, systemdefaults.Smskey, getUser.PhoneNumber, message);

                if (smsResponse == "Sent")
                {
                    return new ObjectResult(new
                    {
                        message = "Code was sent succesfully",
                        userId = getUser.Id,
                        code = getUser.ForgotPasswordCode
                    });
                }
                else
                {
                    return BadRequest(new { smserror = smsResponse });
                }
            }
            string forgotpasswordcode = Convert.ToString(random.Next(1000, 9999));
            int response = GenerateCode(getUser, forgotpasswordcode);
            if(response > 0)
            {
                // verify sms
                string message = $"Your verification code is {forgotpasswordcode}";
                var systemdefaults = await _context.Systemdefaults.FirstOrDefaultAsync();
                var smsResponse = SmsService.VerifyAccount(systemdefaults.SmsuserId, systemdefaults.Smskey, getUser.PhoneNumber, message);

                if (smsResponse == "Sent")
                {
                    return new ObjectResult(new
                    {
                        message = "Code was sent succesfully",
                        userId = getUser.Id,
                        code = forgotpasswordcode
                    });
                }
                else
                {
                    return BadRequest(new { smserror = smsResponse });
                }
            }
            else
            {
                return BadRequest(new {message = "Please try again later" });
            }
        }

        [HttpGet("ResendforgotCode")]
        public async Task<IActionResult> ResendforgotCode(string phonenumber)
        {
            try
            {

                var userPhone = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phonenumber);
                if (userPhone == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (userPhone.ForgotPasswordCode !=null)
                {

                    // verify sms
                    string sms = $"Your verification code is {userPhone.ForgotPasswordCode}";
                    var systemdefaultsx = await _context.Systemdefaults.FirstOrDefaultAsync();
                    var responsex = SmsService.VerifyAccount(systemdefaultsx.SmsuserId, systemdefaultsx.Smskey, userPhone.PhoneNumber, sms);

                    if (responsex == "Sent")
                    {
                        return new ObjectResult(new
                        {
                            message = "Code was resent succesfully",
                            userId = userPhone.Id,
                            code = userPhone.ForgotPasswordCode
                        });
                    }
                    else
                    {
                        return BadRequest(new { smserror = responsex });
                    }
                }

                string forgotpasswordresendcode = Convert.ToString(random.Next(1000, 9999));
                int codeResponse = GenerateCode(userPhone, forgotpasswordresendcode);
                if (codeResponse > 0)
                {
                    // verify sms
                    string message = $"Your verification code is {forgotpasswordresendcode}";
                    var systemdefaults = await _context.Systemdefaults.FirstOrDefaultAsync();
                    var smsResponse = SmsService.VerifyAccount(systemdefaults.SmsuserId, systemdefaults.Smskey, userPhone.PhoneNumber, message);

                    if (smsResponse == "Sent")
                    {
                        return new ObjectResult(new
                        {
                            message = "Code was sent succesfully",
                            userId = userPhone.Id,
                            code = forgotpasswordresendcode
                        });
                    }
                    else
                    {
                        return BadRequest(new { smserror = smsResponse });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Something went wrong" });
                }

            }
             catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
           
            
        }

        [HttpPost("CreateNewPassword")]
        public async Task<IActionResult> CreateNewPassword(CreateNewPasswordDto createNewPassword)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == createNewPassword.PhoneNumber); // int.Parse(User.FindFirstValue("id"))

                if (getUser == null || createNewPassword.PhoneNumber != getUser.PhoneNumber)
                {
                    return NotFound(new { message = "Phone number does not exist" });
                }
                if (getUser.IsVerified == false)
                {
                    return BadRequest(new { message = "user is not verified" });
                }


                if (createNewPassword.ForgotPasswordCode != getUser.ForgotPasswordCode)
                {
                    return Unauthorized(new { messagge = "Invalid code" });
                }

                getUser.IsVerified = true;
                getUser.VerificationCode = null;
                getUser.ForgotPasswordCode = null;
                getUser.Password = SecurePasswordHasherHelper.Hash(createNewPassword.NewPassword); ;
                //_context.Users.Update(info);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Password was updated succesfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpPost("UpdateProfileImage")]
        public async Task<IActionResult> UpdateProfileImage([FromForm] UpdateImageDto createNewPassword)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(c => c.Id == int.Parse(User.FindFirstValue("id")) && c.PhoneNumber == User.FindFirstValue("phoneNumber"));
                if (getUser == null)
                {
                    return NotFound(new { message = "user not found" });
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

                    // remove current image
                    if (getUser.ImageUrl != null)
                    {
                        string webRootpath = _webHostEnvironment.WebRootPath;
                        string uploadx = webRootpath + WebContants.ProfileImages;
                        var oldFile = Path.Combine(uploadx, Path.GetFileName(getUser.ImageUrl));
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }


                    using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }

                    getUser.ImageUrl = _baseUrl + WebContants.ProfileImages + fileName + extension;
                }
                else
                {
                    getUser.ImageUrl = getUser.ImageUrl;
                }

                await _context.SaveChangesAsync();

                return new ObjectResult(new
                {
                    message = "Image was updated",
                    getUser.Id,
                    getUser.ImageUrl
                });


            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto updateProfileDto)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(c => c.Id == int.Parse(User.FindFirstValue("id")) && c.PhoneNumber == User.FindFirstValue("phoneNumber"));
                if (getUser == null)
                {
                    return NotFound(new { message = "user not found" });
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

                    // remove current image
                    if (getUser.ImageUrl != null)
                    {
                        string webRootpath = _webHostEnvironment.WebRootPath;
                        string uploadx = webRootpath + WebContants.ProfileImages;
                        var oldFile = Path.Combine(uploadx, Path.GetFileName(getUser.ImageUrl));
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }


                    using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }

                    getUser.ImageUrl = _baseUrl + WebContants.ProfileImages + fileName + extension;
                }
                else
                {
                    getUser.ImageUrl = getUser.ImageUrl;
                }
                if(updateProfileDto.FullName != null)
                {
                    getUser.FullName = updateProfileDto.FullName;
                }
                else if(updateProfileDto.FullName == null)
                {
                    getUser.FullName = getUser.FullName;
                }
               

                await _context.SaveChangesAsync();

                return new ObjectResult(new
                {
                    message = "User Name and Image updated",
                    getUser.Id,
                    getUser.ImageUrl,
                    getUser.FullName,
                });


            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [Authorize]
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePassword)
        {
            try
            {
                var getUser = await _context.Users.FirstOrDefaultAsync(c => c.Id == int.Parse(User.FindFirstValue("id")) && c.PhoneNumber == User.FindFirstValue("phoneNumber"));
               
                if (getUser.IsVerified == false)
                {
                    return BadRequest(new { message = "user is not verified" });
                }


                if (!SecurePasswordHasherHelper.Verify(updatePassword.CurrentPassword, getUser.Password))
                {
                    return Unauthorized(new { messagge = "Invalid user information" });
                }

                getUser.Password = SecurePasswordHasherHelper.Hash(updatePassword.NewPassword); ;
                await _context.SaveChangesAsync();

                return Ok(new { message = "New password was set succesfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        private  int GenerateCode(User getUser, string randomNum)
        {
            try
            {
                
                using SqlConnection connection = new(_configuration.GetConnectionString("DevConnectionString"));
                connection.Open();
                string sql = $"update USERS set ForgotPasswordCode = {randomNum} where Id = {getUser.Id} and PhoneNumber = '{getUser.PhoneNumber}'";
                SqlCommand cmd = new SqlCommand(sql, connection);
                int results = cmd.ExecuteNonQuery();

                connection.Close();
                return results;
            }
            catch (Exception)
            {
                return 0;
            }
        }


        private int GenerateVerifciaionCode(User getUser, string randomNum)
        {
            try
            {

                using SqlConnection connection = new(_configuration.GetConnectionString("DevConnectionString"));
                connection.Open();
                string sql = $"update USERS set VerificationCode = {random}, isverified = NULL where Id = {getUser.Id} and PhoneNumber = '{getUser.PhoneNumber}'";
                SqlCommand cmd = new SqlCommand(sql, connection);
                int results = cmd.ExecuteNonQuery();

                connection.Close();
                return results;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int LoginTimeUpdate(User authuser)
        {
            try
            {
                string dateLogin = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");  // 2022/04/25 14:55:34:44
                using SqlConnection connection = new(_configuration.GetConnectionString("DevConnectionString"));
                connection.Open();
                string sql = $"update Users set LastLogin = '{dateLogin}' where Id = {authuser.Id} and phoneNumber = '{authuser.PhoneNumber}'";
                SqlCommand cmd = new SqlCommand(sql, connection);
                int results = cmd.ExecuteNonQuery();

                connection.Close();
                return results;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
