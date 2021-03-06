using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class VerifyPhoneDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int VerificationCode { get; set; }
    }
}
