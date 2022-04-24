using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace PersonalShoppingAPI.Model
{
    public partial class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        [JsonIgnore]
        public int? VerificationCode { get; set; }

        [JsonIgnore]
        public int? ForgotPasswordCode { get; set; }

        public bool? IsVerified { get; set; }
    }
}
