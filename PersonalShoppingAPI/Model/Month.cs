using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace PersonalShoppingAPI.Model
{
    public partial class Month
    {
        public Month()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string MonthName { get; set; }

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }
    }
}
