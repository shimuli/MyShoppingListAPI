using System;
using System.Text.Json.Serialization;

namespace PersonalShoppingAPI.Dto
{
    public class CreateProductDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double UnitOfSale { get; set; }
        public double PricerPerUnit { get; set; }
        public double PackageSize { get; set; }
        public double ProductQuantity { get; set; }
        public string Store { get; set; }
        public double TotalCost { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsFavourite { get; set; }
        public int CateoryId { get; set; }
        public int MonthId { get; set; }
        public int UserId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

    }
}
