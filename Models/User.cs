using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SmartRoomsApp.API.Models
{
    public class User : IdentityUser<int>
    {
        public string RaspHost { get; set; }
        public string RaspUsername { get; set; }
        public string SshBlobName { get; set; }
        public ICollection<Block> Blocks { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}