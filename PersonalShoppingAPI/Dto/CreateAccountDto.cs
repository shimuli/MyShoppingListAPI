using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalShoppingAPI.Dto
{
    public class CreateAccountDto
    {

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }


        [Required]
        public string Password { get; set; }

        public IFormFile Image { get; set; }
    }
}
