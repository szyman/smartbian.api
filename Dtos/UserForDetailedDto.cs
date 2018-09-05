using System.Collections.Generic;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Dtos
{
    public class UserForDetailedDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string RaspHost { get; set; }
        public string RaspUsername { get; set; }
        public ICollection<BlocksForDetailedDto> Blocks { get; set; }
    }
}