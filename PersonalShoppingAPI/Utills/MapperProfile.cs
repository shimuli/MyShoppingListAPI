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
        }
    }
}
