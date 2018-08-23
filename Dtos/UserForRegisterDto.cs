using System.ComponentModel.DataAnnotations;

namespace SmartRoomsApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password min 4 and max 8 characters")]
        public string Password { get; set; }
    }
}