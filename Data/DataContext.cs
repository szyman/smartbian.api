using SmartRoomsApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartRoomsApp.API.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<Value> Values { get; set; }
    }
}