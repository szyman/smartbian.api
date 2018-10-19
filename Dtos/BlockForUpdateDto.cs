namespace SmartRoomsApp.API.Dtos
{
    public class BlockForUpdateDto
    {
        public int Id { get; set; }
        public int Gpio { get; set; }
        public string Title { get; set; }
        public string ScriptFileName { get; set; }
    }
}