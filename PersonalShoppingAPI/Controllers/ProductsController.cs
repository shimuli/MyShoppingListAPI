using AuthenticationPlugin;
using AutoMapper;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Utills;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        Random random = new();
        string _baseUrl;
        public ProductsController(SHOPPINGLISTContext context, IMapper mapper, IHttpContextAccessor httpContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            var request = httpContext.HttpContext.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}";
            _webHostEnvironment = webHostEnvironment;

        }

        [Authorize]
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm]CreateProductDto createProductDto)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.ProductName.Trim().ToLower() == createProductDto.ProductName.Trim().ToLower());
                if (product != null)
                {
                    return BadRequest(new { message = "You another item with same name" });
                }
                var categories = await _context.Categories.FirstOrDefaultAsync(c => c.Id == createProductDto.CateoryId);
                if(categories == null)
                {
                    return BadRequest(new { message = "The category does not exist" });
                }

                var months = await _context.Months.FirstOrDefaultAsync(c => c.Id == createProductDto.MonthId);
                if (months == null)
                {
                    return BadRequest(new { message = "The month does not exist" });
                }

                //get image
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string imageUrl = String.Empty;
                if (files.Count > 0)
                {


                    string upload = webRootPath + WebContants.ProductsImage;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);


                    using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {

                        files[0].CopyTo(filestream);
                    }

                    imageUrl = _baseUrl + WebContants.ProductsImage + fileName + extension;
                }
                else
                {
                    imageUrl = null;
                }
                var addproduct = _mapper.Map<Product>(createProductDto);
                addproduct.UserId = int.Parse(User.FindFirstValue("id"));
                addproduct.DateCreated = DateTime.Now;
                addproduct.TotalCost = createProductDto.ProductQuantity * createProductDto.PricerPerUnit;
                addproduct.ImageUrl = imageUrl;
                addproduct.ProductId = $"PRO-{CategoryCode(_context)}";
                await _context.Products.AddAsync(addproduct);
                await _context.SaveChangesAsync();

                return Ok(addproduct);


            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public static int CategoryCode(SHOPPINGLISTContext context)
        {
            int item = 0;
            var results = context.NextNumber
                .FromSqlInterpolated($"select NextProductNumber AS NextNumber from SYSTEMDEFAULTS  ;Update SYSTEMDEFAULTS set NextProductNumber = SYSTEMDEFAULTS.NextProductNumber + 1");
            foreach (var entry in results)
            {
                item = entry.NextNumber;
            }

            return item;
        }
    }
}
