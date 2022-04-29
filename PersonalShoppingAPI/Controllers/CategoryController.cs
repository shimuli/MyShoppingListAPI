using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(SHOPPINGLISTContext context, IMapper mapper, ILogger<CategoryController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;

        }

        [Authorize]
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                if (categories.Count > 0)
                {
                    return Ok(categories);
                }
                else
                {
                    return NotFound(new { message = "No data to display" });
                }

            }
            catch (System.Exception ex)
            {
                _logger.LogError("GetCategories: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategory)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = ModelState.Values.ToString() });
                }
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == createCategory.Name.Trim().ToLower());
                //if (category != null)
                //{
                //    return BadRequest(new { message = "category already exists" });
                //}

                var addCategory = _mapper.Map<Category>(createCategory);
                await _context.Categories.AddAsync(addCategory);
                await _context.SaveChangesAsync();
                return Ok(addCategory);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("CreateCategory: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("EditCategory")]
        public async Task<IActionResult> EditCategory(int id, CreateCategoryDto editCategory)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = ModelState.Values.ToString() });
                }
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                if (editCategory.Name == null)
                {
                    category.Name = category.Name;
                }
                if (editCategory.Description == null)
                {
                    category.Description = category.Description;
                }

                if (editCategory.Name != null)
                {
                    category.Name = editCategory.Name;
                }
                if (editCategory.Description != null)
                {
                    category.Description = editCategory.Description;
                }

                await _context.SaveChangesAsync();
                return new ObjectResult(new
                {
                    message = "Category was updated",
                    category.Id,
                    editCategory.Name,
                    editCategory.Description,
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError("EditCategory: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var item = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (item == null)
                {
                    return NotFound(new { message = "Category not found" });
                }
                _context.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Category was deleted" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError("DeleteCategory: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
          
        }
    }
}
