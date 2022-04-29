using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalShoppingAPI.Dto
{
    public class CreateProductDto
    {
        

        [Required]
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        [Required]
        public string UnitOfSale { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price Per Unit must be greater than 0")]
        public double PricerPerUnit { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Package Size must be greater than 0")]
        public double PackageSize { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Product Quantity must be greater than 0")]
        public double ProductQuantity { get; set; }
        public string Store { get; set; }

        public bool IsFavourite { get; set; } = false;

        public DateTime ExpiryDate { get; set; }

        [Required]
        public int CateoryId { get; set; }

        [Required]
        public int MonthId { get; set; }

        [Required]
        public bool IsRecurring { get; set; }
        public IFormFile Image { get; set; }

    }
}
