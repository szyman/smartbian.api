using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SmartRoomsApp.API.Models;
using SmartRoomsApp.API.Service;

namespace SmartRoomsApp.API.Data
{
    public class WebsocketHandler
    {
        private readonly ICombiningRepository _repo;
        private readonly SshService _ssh;
        public WebsocketHandler(ICombiningRepository repo, SshService ssh)
        {
            this._repo = repo;
            this._ssh = ssh;
        }

        public async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string receivedString = Encoding.ASCII.GetString(buffer, 0, result.Count);
            string[] receivedData = receivedString.Split('/');
            int userId = Int32.Parse(receivedData[0]);
            int blockId = Int32.Parse(receivedData[1]);
            User user = await this._repo.GetUser(userId);

            using (var sshClient = await this._ssh.getSshClient(user))
            {
                try
                {
                    var scriptFileName = await this._repo.GetBlock(blockId);
                    if (!sshClient.IsConnected)
                    {
                        sshClient.Connect();
                    }

                    while (!result.CloseStatus.HasValue)
                    {
                        var resultFile = this._ssh.executeCommand(sshClient, SshService.CommandTypeEnum.RunScript, scriptFileName.ScriptFileName);
                        byte[] resultBytes = Encoding.ASCII.GetBytes(resultFile.Result);

                        await webSocket.SendAsync(new ArraySegment<byte>(resultBytes, 0, resultBytes.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(resultBytes), CancellationToken.None);
                    }
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
                }
            }

        }
    }
}
