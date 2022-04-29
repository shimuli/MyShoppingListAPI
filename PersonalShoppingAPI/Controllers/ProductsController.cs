using AuthenticationPlugin;
using AutoMapper;
using Faker;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        Random random = new();
        string _baseUrl;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(ILogger<ProductsController> logger,SHOPPINGLISTContext context, IMapper mapper, IHttpContextAccessor httpContext, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            var request = httpContext.HttpContext.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}";
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;

        }

        
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
                addproduct.IsActive = true;
                addproduct.UpdatedQuantity = addproduct.ProductQuantity;
                addproduct.ProductId = $"PRO-{CategoryCode(_context)}";
                if(addproduct.ExpiryDate == DateTime.MinValue || addproduct.ExpiryDate == null)
                {
                    addproduct.ExpiryDate = null;
                }
                await _context.Products.AddAsync(addproduct);
                await _context.SaveChangesAsync();

                return Ok(addproduct);


            }
            catch (Exception ex)
            {
                _logger.LogError("CreateProduct: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("UserProducts")]
        public async Task<IActionResult> UserProducts(string productName)
        {
            try
            {
               
                  var items = string.IsNullOrEmpty(productName) ? await _context.Products
                    .Where(c => c.UserId == int.Parse(User.FindFirstValue("id"))).ToListAsync()

                    : await _context.Products.Where(c => c.ProductName.Contains(productName))
                    .Where(c => c.UserId == int.Parse(User.FindFirstValue("id"))).ToListAsync();

                if (items.Count > 0)
                {
                    return Ok(items);
                }
                else
                {

                    return NotFound(new { message = "No iems found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UserProducts: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("GetExpiringProducts")]
        public async Task<IActionResult> GetExpiringProducts(DateTime date)
        {
            try
            {
                if(date == DateTime.MinValue)
                {
                    var getUser = await _context.Users.FirstOrDefaultAsync(c => c.Id == int.Parse(User.FindFirstValue("id")));
                    var expiryday = getUser.ProductExNotificaionDay;

                    var x = DateTime.Today.AddDays((double)expiryday + 1);

                    var items = await _context.Products
                        .Where(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.ExpiryDate <= DateTime.Today.AddDays((double)(expiryday + 1)) && c.UpdatedQuantity > 0).ToListAsync();

                    if (items.Count > 0)
                    {
                        return Ok(items);
                    }
                    else
                    {

                        return NotFound(new { message = "No iems found" });
                    }
                }
                else
                {
                    TimeSpan diff = Convert.ToDateTime(date) - DateTime.Now;
                    double days = diff.Days;

                    var x = DateTime.Today.AddDays(days + 1);

                    var items = await _context.Products
                        .Where(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.ExpiryDate <= DateTime.Today.AddDays((days + 1)) && c.UpdatedQuantity >0).ToListAsync();

                    if (items.Count > 0)
                    {
                        return Ok(items);
                    }
                    else
                    {

                        return NotFound(new { message = "No iems found" });
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError("GetExpiringProducts: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts(string productName)
        {
            try
            {
                var items = string.IsNullOrEmpty(productName) ? await _context.Products.ToListAsync()
                    : await _context.Products.Where(c => c.ProductName.Contains(productName)).ToListAsync();

                if (items.Count > 0)
                {
                    return Ok(items);
                }
                else
                {
                    return NotFound(new { message = "No item found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProducts: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int id,[FromForm] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")));


                if (product == null)
                {
                    return NotFound(new { message = "Item not found" });
                }

                if (product.ProductQuantity < product.UpdatedQuantity)
                {
                    return BadRequest(new { message = "new quantity cannot be less than updated quantity" });
                }

                var categories = await _context.Categories.FirstOrDefaultAsync(c => c.Id == createProductDto.CateoryId);
                if (categories == null)
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

                    // remove current image
                    if (product.ImageUrl != null)
                    {
                        string webRootpath = _webHostEnvironment.WebRootPath;
                        string uploadx = webRootpath + WebContants.ProductsImage;
                        var oldFile = Path.Combine(uploadx, Path.GetFileName(product.ImageUrl));
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }


                    using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {

                        files[0].CopyTo(filestream);
                    }

                    imageUrl = _baseUrl + WebContants.ProductsImage + fileName + extension;
                }
                else
                {
                    imageUrl = product.ImageUrl;
                }
                var addproduct = _mapper.Map<Product>(createProductDto);
                product.UserId = int.Parse(User.FindFirstValue("id"));
                product.DateCreated = product.DateCreated;
                product.DateUpdated = DateTime.Now;
                product.TotalCost = createProductDto.ProductQuantity * createProductDto.PricerPerUnit;
                product.ImageUrl = imageUrl;
                product.IsActive = true;
                product.UpdatedQuantity = addproduct.ProductQuantity;
                product.ProductId = product.ProductId;
                product.ProductName = addproduct.ProductName;
                product.ProductDescription = addproduct.ProductDescription;
                product.UnitOfSale = addproduct.UnitOfSale;
                product.PricerPerUnit = addproduct.PricerPerUnit;
                product.PackageSize = addproduct.PackageSize;
                product.ProductQuantity = addproduct.ProductQuantity;
                product.Store = addproduct.Store;
                product.IsActive = true;
                product.IsFavourite = addproduct.IsFavourite;
                product.CateoryId = addproduct.CateoryId;
                product.MonthId = addproduct.MonthId;

                if (addproduct.ExpiryDate == DateTime.MinValue)
                {
                    addproduct.ExpiryDate = null;
                }

               await _context.SaveChangesAsync();

                return Ok(product);


            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateProduct: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateFavourite")]
        public async Task<IActionResult> UpdateFavourite(int id)
        {
            try
            {
                var item = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.Id == id);
                if(item == null)
                {
                    return NotFound(new {message = "Product not found"});
                }

                if (item.IsFavourite)
                {
                    item.IsFavourite = false;
                }
                else
                {
                    item.IsFavourite = true;
                    
                }
              
                item.DateUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(item);

            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateFavourite: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            try
            {
                var item = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.Id == id);
                if (item == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                if (item.IsActive)
                {
                    item.IsActive = false;
                    
                }
                else
                {
                    item.IsActive = true;
                    
                }

                item.DateUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(item);

            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateStatus: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int id, double removedQuantity)
        {
            try
            {
                if(removedQuantity == null || removedQuantity <= 0)
                {
                    return BadRequest(new { message = "Invalid quantity" });
                }
                var item = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.Id == id);
                if (item == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                if(removedQuantity > item.UpdatedQuantity)
                {
                    return BadRequest(new { message = "You cannot remove more than you have" });
                }
                item.UpdatedQuantity = item.ProductQuantity - removedQuantity;
                item.DateUpdated = DateTime.Now;
                return Ok(item);

            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateQuantity: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var item = await _context.Products.FirstOrDefaultAsync(c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.Id == id);
                if (item == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
                if(item.UpdatedQuantity > 0)
                {
                    return BadRequest(new { message = "Product still has quantity" });
                }

                // remove  image
                if (item.ImageUrl != null)
                {
                    string webRootpath = _webHostEnvironment.WebRootPath;
                    string uploadx = webRootpath + WebContants.ProductsImage;
                    var oldFile = Path.Combine(uploadx, Path.GetFileName(item.ImageUrl));
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                _context.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "product deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteProduct: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("GenerateShoppingList")]
        public async Task<IActionResult> GenerateShoppingList(double minimumQuantity)
        {
            try
            {
                var item = await _context.Products.Where( c => c.UserId == int.Parse(User.FindFirstValue("id")) && c.UpdatedQuantity <= minimumQuantity && c.IsRecurring == true).ToListAsync();
                if(item.Count > 0)
                { 
                    return Ok(item);
                }
                return NotFound(new { messag = "No item to display" });
            }
            catch (Exception ex)
            {
                _logger.LogError("GenerateShoppingList: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            int items = 20;
            Product product = new();
            for (int i = 1; i < items+1; i++)
            {
                product.UserId = int.Parse(User.FindFirstValue("id"));
                product.DateCreated = DateTime.Now;
                product.DateUpdated = null;
                product.ProductQuantity = i+4;
                product.PricerPerUnit = i * 160;
                product.TotalCost = product.ProductQuantity * product.PricerPerUnit;
                product.ImageUrl = "https://localhost:44325/images/products/64f6f9bd-e1b5-44db-8fea-005c8b83cc65.jpg";
                product.IsActive = true;
                product.UpdatedQuantity = product.ProductQuantity;
                product.ProductId = $"PRO-{CategoryCode(_context)}";
                product.ProductName = Company.Name();
                product.ProductDescription = Lorem.Sentence();
                product.UnitOfSale = "Any";
                product.PackageSize = i;
                product.Store = Company.Name();
                product.IsActive = true;
                product.IsFavourite = false;
                product.CateoryId = 4044;
                product.MonthId = 5;
                product.IsRecurring = true;
                product.ExpiryDate = DateTime.Today.AddMonths(i + 6);
                await _context.AddAsync(product);
                await _context.SaveChangesAsync();

                product.Id = 0;

            }

              return Ok(new {message = "Products added" });
            
            
        }

        public static int CategoryCode(SHOPPINGLISTContext context)
        {
            try
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
            catch (Exception ex)
            {
                return -1;
            }
           
        }
    }
}
