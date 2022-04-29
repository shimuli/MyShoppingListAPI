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
    public class MonthsController : ControllerBase
    {
        private readonly SHOPPINGLISTContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MonthsController> _logger;

        public MonthsController(SHOPPINGLISTContext context, IMapper mapper, ILogger<MonthsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;

        }
        
        [Authorize]
        [HttpGet("GetMonths")]
        public async Task<IActionResult> GetMonths()
        {
            try
            {
               // throw new System.Exception();
                var months = await _context.Months.ToListAsync();
                if(months.Count > 0)
                {
                    return Ok(months);
                }
                else
                {
                    return NotFound(new { message = "No data to display" });
                }
                
            }
            catch (System.Exception ex)
            {
                _logger.LogError("GetMonths: " + ex, ex.Message);
                return BadRequest(new {message =ex.Message});
            }                
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateMonth")]
        public async Task<IActionResult> CreateMonth(CreateMonths createMonths)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = ModelState.Values.ToString() });
                }
                var months = await _context.Months.FirstOrDefaultAsync(c => c.MonthName.Trim().ToLower() == createMonths.MonthName.Trim().ToLower());
                if (months != null)
                {
                    return BadRequest(new { message = "month already exists" });
                }

                var addmonth = _mapper.Map<Month>(createMonths);
                await _context.Months.AddAsync(addmonth);
                await _context.SaveChangesAsync();
                return Ok(addmonth);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("CreateMonth: " + ex, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            
        }

    }
}
