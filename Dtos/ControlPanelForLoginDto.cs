namespace SmartRoomsApp.API.Dtos
{
    public class ControlPanelForLoginDto
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CommandType { get; set; }
        public int ItemId { get; set; }
    }
}