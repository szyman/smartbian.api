namespace SmartRoomsApp.API.Dtos
{
    public class ControlPanelForLoginDto
    {
        public int UserId { get; set; }
        public string CommandType { get; set; }
        public int ItemId { get; set; }
        public bool IsRequireScript { get; set; }
    }
}