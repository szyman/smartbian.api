using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            { "test_connection", "python -V" },
            { "run_switch", "python test.py" }
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

        [HttpPost("uploadScriptFile")]
        public IActionResult uploadScriptFile(ControlPanelForLoginDto controlPanelForLogin)
        {
            using (var client = new SftpClient(controlPanelForLogin.Host, controlPanelForLogin.Username, controlPanelForLogin.Password))
            {
                try
                {
                    string scriptFileName = "test.py";
                    bool isFileCreated = false;
                    client.Connect();
                    IEnumerable<string> files = client.ListDirectory("").Select(s => s.Name);

                    foreach (string fileName in files)
                    {
                        if (fileName == scriptFileName)
                        {
                            isFileCreated = true;
                            break;
                        }
                    }

                    if (!isFileCreated)
                    {
                        var stream = new MemoryStream();
                        var writer = new StreamWriter(stream);
                        writer.Write("print(\"This line will be printed.\")");

                        writer.Flush();
                        stream.Position = 0;

                        client.UploadFile(stream, "test.py");
                    }
                    return Ok("File: " + scriptFileName + " exist: " + isFileCreated);
                }
                catch (SocketException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}