﻿using Microsoft.AspNetCore.Http;
namespace PersonalShoppingAPI.Dto
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; }

        public IFormFile Image { get; set; }
    }
}
