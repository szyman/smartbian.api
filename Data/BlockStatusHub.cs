using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Models;
using SmartRoomsApp.API.Service;

public class BlockStatusHub : Hub
{
    private readonly ICombiningRepository _repo;
    private readonly SshService _ssh;
    public BlockStatusHub(ICombiningRepository repo, SshService ssh)
    {
        this._repo = repo;
        this._ssh = ssh;
    }
    public Task GetBlockStatus(int userId, int blockId)
    {
        User user = this._repo.GetUser(userId).GetAwaiter().GetResult();
        Block block = this._repo.GetBlock(blockId).GetAwaiter().GetResult();

        if (block.ScriptFileName.Length == 0)
        {
            return Clients.Caller.SendAsync("ReceiveBlockStatus", null);
        }

        using (var sshClient = this._ssh.getSshClient(user).GetAwaiter().GetResult())
        {
            try
            {
                if (!sshClient.IsConnected)
                {
                    sshClient.Connect();
                }

                var resultFile = this._ssh.executeCommand(sshClient, SshService.CommandTypeEnum.RunScript, block.ScriptFileName);
                return Clients.Caller.SendAsync("ReceiveBlockStatus", resultFile);
            }
            catch (Exception ex)
            {
                return Clients.Caller.SendAsync("ReceiveBlockStatusError", ex.Message.Substring(0, 50));
            }
        }
    }


}