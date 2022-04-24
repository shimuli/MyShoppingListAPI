using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class UpdateImageDto
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
