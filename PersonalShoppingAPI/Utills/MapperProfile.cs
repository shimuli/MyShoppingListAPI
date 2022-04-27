using AutoMapper;
using PersonalShoppingAPI.Dto;
using PersonalShoppingAPI.Model;

namespace PersonalShoppingAPI.Utills
{
    public class MapperProfile :Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UsersDto>().ReverseMap();
            CreateMap<User, CreateAccountDto>().ReverseMap(); 
            CreateMap<User, AuthDto>().ReverseMap();
            CreateMap<User, UpdateProfileDto>().ReverseMap();
            CreateMap<User, CreateAdminDto>().ReverseMap();
            CreateMap<Month, CreateMonths>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Product, CreateProductDto > ().ReverseMap();
        }
    }
}
