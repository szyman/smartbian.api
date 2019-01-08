using System.Threading.Tasks;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Data
{
    public interface IAuthRepository
    {
         Task<User> Register(User user, string password);
         Task<User> Login(string username, string password);
         Task<User> LoginExtProvider(string username);
         Task<bool> UserExists(string username);
    }
}