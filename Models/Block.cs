namespace SmartRoomsApp.API.Models
{
    public class Block
    {
        public int Id { get; set; }
        public int DataX { get; set; }
        public int DataY { get; set; }
        public string Style { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public string ScriptFileName { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
