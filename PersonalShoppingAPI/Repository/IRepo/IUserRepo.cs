using PersonalShoppingAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Repository.IRepo
{
    public interface IUserRepo
    {
        Task <List<User>> GetUsersAsync();
    }
}
