using System.ComponentModel.DataAnnotations;

namespace SmartRoomsApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "You must specify Username min 3 and max 20 characters")]
        public string Username { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "You must specify Password min 3 and max 20 characters")]
        public string Password { get; set; }
    }
}