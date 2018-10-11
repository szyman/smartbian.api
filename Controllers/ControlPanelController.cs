using System.Collections.Generic;
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
        private static readonly Dictionary<string, string> _COMMAND_TYPES = new Dictionary<string, string>()
        {
            { "test_connection", "python -V" }
        };

        [HttpPost("executeCommand")]
        public IActionResult executeCommand(ControlPanelForLoginDto controlPanelForLogin)
        {
            using (var client = new SshClient(controlPanelForLogin.Host, controlPanelForLogin.Username, controlPanelForLogin.Password))
            {
                try
                {
                    client.Connect();
                    SshCommand command = client.CreateCommand(_COMMAND_TYPES[controlPanelForLogin.CommandType]);
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