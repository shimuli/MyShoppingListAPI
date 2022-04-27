using System.ComponentModel.DataAnnotations;

namespace PersonalShoppingAPI.Dto
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
