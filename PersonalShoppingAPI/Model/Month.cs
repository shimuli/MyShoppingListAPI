using System;
using System.Collections.Generic;

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

        public virtual ICollection<Product> Products { get; set; }
    }
}
