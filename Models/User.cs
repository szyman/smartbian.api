using System.Collections.Generic;

namespace SmartRoomsApp.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RaspHost { get; set; }
        public string RaspUsername { get; set; }
        public string SshBlobName { get; set; }
        public ICollection<Block> Blocks { get; set; }
    }
}