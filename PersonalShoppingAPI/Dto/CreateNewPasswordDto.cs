using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class CreateNewPasswordDto
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int ForgotPasswordCode { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
