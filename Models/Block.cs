namespace SmartRoomsApp.API.Models
{
    public class Block
    {
        public int Id { get; set; }
        public int DataX { get; set; }
        public int DataY { get; set; }
        public string Width { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}