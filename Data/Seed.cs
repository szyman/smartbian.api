using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Data
{
    public class Seed
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public Seed(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedUSers()
        {
            if (_userManager.Users.Any())
            {
                return;
            }

            var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);

            var roles = new List<Role>
            {
                new Role{Name = "Member"},
                new Role{Name = "Admin"}
            };

            foreach (var role in roles)
            {
                _roleManager.CreateAsync(role).Wait();
            }

            foreach (var user in users)
            {
                _userManager.CreateAsync(user, "haslo").Wait();

                if (user.UserName == "admin")
                {
                    _userManager.AddToRolesAsync(user, new[] {"Member", "Admin"}).Wait();
                } else
                {
                    _userManager.AddToRoleAsync(user, "Member").Wait();
                }
            }
        }
    }
}