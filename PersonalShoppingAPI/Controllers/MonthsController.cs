using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public MonthsController(SHOPPINGLISTContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }
        
        [Authorize]
        [HttpGet("GetMonths")]
        public async Task<IActionResult> GetMonths()
        {
            try
            {
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
                return BadRequest(new {message =ex.Message});
            }                
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateMonth")]
        public async Task<IActionResult> CreateMonth(CreateMonths createMonths)
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

    }
}
