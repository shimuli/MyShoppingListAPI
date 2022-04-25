using System;
using System.Text.Json.Serialization;

namespace PersonalShoppingAPI.Dto
{
    public class CreateAdminDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }

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
        public int? ForgotPasswordCode { get; set; }

        [JsonIgnore]
        public bool? IsVerified { get; set; }

    }
}
