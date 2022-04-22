using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class VerifyPhoneDto
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int VerificationCode { get; set; }
    }
}
