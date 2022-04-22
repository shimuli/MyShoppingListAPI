using System;
using System.Collections.Generic;

#nullable disable

namespace PersonalShoppingAPI.Model
{
    public partial class Systemdefault
    {
        public int Id { get; set; }
        public string SmsuserId { get; set; }
        public string Smskey { get; set; }
        public string Smsname { get; set; }
        public int? NextUserNumber { get; set; }
        public int? NextProductNumber { get; set; }
    }
}
