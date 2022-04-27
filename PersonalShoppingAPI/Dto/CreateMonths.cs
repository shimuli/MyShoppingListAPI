using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class CreateMonths
    {
        [Required]
        public string MonthName { get; set; }
    }
}
