using Microsoft.EntityFrameworkCore;
using PersonalShoppingAPI.Model;
using PersonalShoppingAPI.Repository.IRepo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Repository.Repo
{
    public class UserRepo : IUserRepo
    {
        private readonly SHOPPINGLISTContext _context;
        public UserRepo(SHOPPINGLISTContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }
    }
}
