using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using SmartRoomsApp.API.Dtos;

namespace SmartRoomsApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPanelController : ControllerBase
    {
        [HttpPost("testConnection")]
        public IActionResult testConnection(ControlPanelForLoginDto controlPanelForLogin)
        {
            using (var client = new SshClient(controlPanelForLogin.Host, controlPanelForLogin.Username, controlPanelForLogin.Password))
            {
                try
                {
                    client.Connect();
                    SshCommand command = client.CreateCommand("python -V");
                    command.Execute();

                    return Ok(command.Result + command.Error);
                }
                catch (SocketException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}