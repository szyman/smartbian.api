namespace SmartRoomsApp.API.Features.Blocks.GetItem
{
    public class GetItemResult
    {
        public int Id { get; set; }
        public int DataX { get; set; }
        public int DataY { get; set; }
        public string Style { get; set; }
        public int Type { get; set; }
        public int Gpio { get; set; }
        public string Title { get; set; }
        public int SocketPort { get; set; }
        public string ScriptFileName { get; set; }
        public int UserId { get; set; }
    }
}