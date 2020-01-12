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
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace SmartRoomsApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPanelController : ControllerBase
    {
        private readonly string _rtmpServerHost;
        private readonly string _rtmpPlaybackHost;
        private readonly ICombiningRepository _repo;
        private readonly ICloudStorageRepository _cloudStorage;

        public ControlPanelController(IHostingEnvironment env, ICombiningRepository repo, ICloudStorageRepository cloudStorage)
        {
            if (env.IsDevelopment())
            {
                this._rtmpServerHost = "rtmp://192.168.100.10:1935";
                this._rtmpPlaybackHost = "http://192.168.100.10:8000";
            }
            else
            {
                this._rtmpServerHost = "rtmp://smartbian.westeurope.cloudapp.azure.com:1935";
                this._rtmpPlaybackHost = "http://smartbian.westeurope.cloudapp.azure.com:8000";
            }
            this._repo = repo;
            this._cloudStorage = cloudStorage;
        }

        [HttpPost("getVideoLink")]
        public IActionResult getVideoLink(ControlPanelForLoginDto controlPanelForLogin)
        {
            if (controlPanelForLogin.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            string url = $"{_rtmpPlaybackHost}/live/stream_{controlPanelForLogin.UserId}_{controlPanelForLogin.ItemId}.flv";
            return Ok(url);

        }

        [HttpPost("executeCommand")]
        public async Task<IActionResult> executeCommand(ControlPanelForLoginDto controlPanelForLogin)
        {
            PrivateKeyFile privateKeyFile;

            if (controlPanelForLogin.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            User user = await _repo.GetUser(controlPanelForLogin.UserId);

            try
            {
                var stream = await _cloudStorage.downloadStreamFromBlobContainer(user.SshBlobName);
                privateKeyFile = new PrivateKeyFile(stream);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (var client = new SshClient(user.RaspHost, user.RaspUsername, privateKeyFile))
            {
                try
                {
                    var commandText = await this._getCommandAsync(controlPanelForLogin.CommandType, controlPanelForLogin.ItemId, controlPanelForLogin.UserId);
                    if (String.IsNullOrEmpty(commandText))
                    {
                        return Ok("Missing script.");
                    }

                    client.Connect();
                    SshCommand command = client.CreateCommand(commandText);
                    this._executeCommand(controlPanelForLogin.CommandType, command);
                    var result = command.Result + command.Error;
                    if (String.IsNullOrEmpty(result) && controlPanelForLogin.IsRequireScript)
                    {
                        return Ok("Missing message. Add print() to script item.");
                    }

                    return Ok(command.Result + command.Error);
                }
                catch (SocketException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        private async Task<string> _getCommandAsync(string CommandType, int itemId, int userId)
        {
            switch (CommandType)
            {
                case "test_connection":
                    return "python -V";
                case "run_switch":
                    Block block = await _repo.GetBlock(itemId);
                    if (block == null || String.IsNullOrEmpty(block.ScriptFileName))
                        return "";
                    return "python " + block.ScriptFileName;
                case "video_streaming":
                    string secondPartLink = $"/live/stream_{userId}_{itemId}";
                    return "raspivid -o - -t 0 -hf -w 640 -h 360 -fps 25|cvlc -vvv stream:///dev/stdin --sout '#standard{access=http,mux=ts,dst=:8090}' :demux=h264 | ffmpeg -i http://localhost:8090 -vcodec libx264 -f flv -r 25 -an " + _rtmpServerHost + secondPartLink;
                case "video_status":
                    return "pidof raspivid ffmpeg";
                case "video_stop":
                    return "kill -9 `pidof raspivid ffmpeg`";
                default:
                    return "";
            }
        }

        private void _executeCommand(string commandType, SshCommand command)
        {
            if (commandType == "video_streaming")
            {
                var asyncExecute = command.BeginExecute();
                command.EndExecute(asyncExecute);
            }
            else
            {
                command.Execute();
            }
        }

        private string _getMd5Hash(string text)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        private long _getTimestamp()
        {
            DateTime tomorrow = DateTime.UtcNow.AddDays(1);
            return ((DateTimeOffset)tomorrow).ToUnixTimeSeconds();
        }
    }
}