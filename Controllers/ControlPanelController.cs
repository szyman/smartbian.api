using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Dtos;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPanelController : ControllerBase
    {
        private readonly ICombiningRepository _repo;

        public ControlPanelController(ICombiningRepository repo)
        {
            this._repo = repo;
        }

        private static readonly Dictionary<string, string> _COMMAND_TYPES = new Dictionary<string, string>()
        {
            { "test_connection", "python -V" },
            { "run_switch", "python switch_lamp.py" }
        };

        [HttpPost("executeCommand")]
        public async Task<IActionResult> executeCommand(ControlPanelForLoginDto controlPanelForLogin)
        {
            PrivateKeyFile privateKeyFile;

            if (controlPanelForLogin.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                // TODO: Fix on the cloud
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            User user = await _repo.GetUser(controlPanelForLogin.UserId);

            using (var client = new SshClient(user.RaspHost, user.RaspUsername, privateKeyFile))
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
        public async Task<IActionResult> uploadScriptFile(ControlPanelForLoginDto controlPanelForLogin)
        {
            PrivateKeyFile privateKeyFile;

            if (controlPanelForLogin.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            User user = await _repo.GetUser(controlPanelForLogin.UserId);

            using (var client = new SftpClient(user.RaspHost, user.RaspUsername, privateKeyFile))
            {
                try
                {
                    string scriptFileName = "switch_lamp.py";
                    bool isFileExist = false;
                    client.Connect();
                    IEnumerable<string> files = client.ListDirectory("").Select(s => s.Name);

                    foreach (string fileName in files)
                    {
                        if (fileName == scriptFileName)
                        {
                            isFileExist = true;
                            break;
                        }
                    }

                    using (var fileStream = new FileStream("./Assets/Scripts/switch_lamp.py", FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Flush();
                        fileStream.Position = 0;
                        client.UploadFile(fileStream, "switch_lamp.py");
                        fileStream.Close();
                    }

                    return Ok("File: " + scriptFileName + " Replaced: " + isFileExist);
                }
                catch (SocketException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}