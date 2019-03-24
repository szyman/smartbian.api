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
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPanelController : ControllerBase
    {
        private readonly ICombiningRepository _repo;
        private readonly ICloudStorageRepository _cloudStorage;

        public ControlPanelController(ICombiningRepository repo, ICloudStorageRepository cloudStorage)
        {
            this._repo = repo;
            this._cloudStorage = cloudStorage;
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
                    var commandText = await this._getCommandAsync(controlPanelForLogin.CommandType, controlPanelForLogin.ItemId);
                    client.Connect();
                    SshCommand command = client.CreateCommand(commandText);
                    command.Execute();

                    return Ok(command.Result + command.Error);
                }
                catch (SocketException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        private async Task<string> _getCommandAsync(string CommandType, int itemId)
        {
            switch(CommandType)
            {
                case "test_connection":
                    return "python -V";
                case "run_switch":
                    Block block = await _repo.GetBlock(itemId);
                    if (block == null || block.ScriptFileName == null)
                        return "";
                    return "python " + block.ScriptFileName;
                default:
                    return "";
            }
        }
    }
}