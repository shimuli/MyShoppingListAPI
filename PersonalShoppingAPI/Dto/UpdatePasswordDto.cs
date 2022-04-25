using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class UpdatePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
