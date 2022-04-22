using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalShoppingAPI.Dto
{
    public class CreateAccountDto
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [JsonIgnore]
        public string ImageUrl { get; set; }

        [Required]
        public string Password { get; set; }

        [JsonIgnore]
        public string Role { get; set; }

        [JsonIgnore]
        public bool IsActive { get; set; }
        [JsonIgnore]
        public DateTime? DateCreated { get; set; }
        [JsonIgnore]
        public DateTime? DateUpdated { get; set; }

        [JsonIgnore]
        public int? VerificationCode { get; set; }

        [JsonIgnore]
        public bool? IsVerified { get; set; }
    }
}
