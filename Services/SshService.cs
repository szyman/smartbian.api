using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Renci.SshNet;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Service
{
    public class SshService
    {
        public enum CommandTypeEnum { TestConnection, RunScript }
        private readonly ICloudStorageRepository _cloudStorage;

        public SshService(ICloudStorageRepository cloudStorage)
        {
            this._cloudStorage = cloudStorage;
        }

        public async Task<SshClient> getSshClient(User user)
        {
            PrivateKeyFile privateKeyFile;
            try
            {
                var stream = await _cloudStorage.downloadStreamFromBlobContainer(user.SshBlobName);
                privateKeyFile = new PrivateKeyFile(stream);

                return new SshClient(user.RaspHost, user.RaspUsername, privateKeyFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public SshCommand executeCommand(SshClient client, CommandTypeEnum commandType, string scriptFileName)
        {
            try
            {
                var commandText = this._getCommand(commandType, scriptFileName);
                SshCommand command = client.CreateCommand(commandText);
                command.Execute();

                return command;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string _getCommand(CommandTypeEnum CommandType, string scriptFileName)
        {
            switch (CommandType)
            {
                case CommandTypeEnum.TestConnection:
                    return "python -V";
                case CommandTypeEnum.RunScript:
                    if (scriptFileName == null)
                        return "";
                    return "python " + scriptFileName;
                default:
                    return "";
            }
        }
    }
}
