using System.Collections.Generic;
using System.Threading.Tasks;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Data
{
    public interface ICombiningRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
         Task<bool> SaveAll();
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
        Task<Block> GetBlock(int id);
    }
}