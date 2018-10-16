namespace SmartRoomsApp.API.Dtos
{
    public class BlocksForDetailedDto
    {
        public int Id { get; set; }
        public int DataX { get; set; }
        public int DataY { get; set; }
        public string Style { get; set; }
        public int Type { get; set; }
        public int Gpio { get; set; }
        public string Title { get; set; }
        public string ScriptFileName { get; set; }
        public int UserId { get; set; }
    }
}