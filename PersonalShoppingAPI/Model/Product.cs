using System;
using System.Collections.Generic;

#nullable disable

namespace PersonalShoppingAPI.Model
{
    public partial class Product
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string UnitOfSale { get; set; }
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

        public virtual Category Cateory { get; set; }
        public virtual Month Month { get; set; }
    }
}
