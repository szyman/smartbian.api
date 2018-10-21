using System;
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
            { "run_switch", "python switch_lamp.py" }
        };

        [HttpPost("executeCommand")]
        public IActionResult executeCommand(ControlPanelForLoginDto controlPanelForLogin)
        {
            PrivateKeyFile privateKeyFile;
            try
            {
                // TODO: Fix on the cloud
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (var client = new SshClient(controlPanelForLogin.Host, controlPanelForLogin.Username, privateKeyFile))
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
            PrivateKeyFile privateKeyFile;
            try
            {
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (var client = new SftpClient(controlPanelForLogin.Host, controlPanelForLogin.Username, privateKeyFile))
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