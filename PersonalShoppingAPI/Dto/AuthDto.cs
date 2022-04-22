using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class AuthDto
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
