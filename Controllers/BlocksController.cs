using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public class BlocksController : ControllerBase
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudStorageRepository _cloudStorage;

        public BlocksController(ICombiningRepository repo, IMapper mapper, ICloudStorageRepository cloudStorage)
        {
            this._repo = repo;
            this._mapper = mapper;
            this._cloudStorage = cloudStorage;
        }

        [HttpGet("{blockId}")]
        public async Task<IActionResult> GetItem(int blockId)
        {
            Block block = await _repo.GetBlock(blockId);

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var blockForUpdate = _mapper.Map<BlockForUpdateDto>(block);
            return Ok(blockForUpdate);
        }

        [HttpDelete("{blockId}")]
        public async Task<IActionResult> DeleteItem(int blockId)
        {
            Block block = await _repo.GetBlock(blockId);
            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                using (var client = await _getConnectedSftpClient(block.User))
                {
                    client.DeleteFile(block.ScriptFileName);
                    _repo.Delete(block);
                    await _repo.SaveAll();

                    return Ok(
                        new { id = block.Id }
                    );
                }
            }
            catch (Exception ex)
            {
                _repo.Delete(block);
                await _repo.SaveAll();

                return Ok(
                    new {
                        id = block.Id,
                        error = ex.Message
                    }
                );
            }
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> GetItems(int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);
            var blocksToReturn = _mapper.Map<IEnumerable<BlocksForDetailedDto>>(user.Blocks);

            return Ok(blocksToReturn);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddItems(int userId, [FromBody] List<BlocksForDetailedDto> blocksForDetailedDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            User user = await _repo.GetUser(userId);
            BlocksForDetailedDto blockForUpdate;
            foreach (var block in user.Blocks)
            {
                blockForUpdate = blocksForDetailedDto.Find(b => b.Id == block.Id);
                _mapper.Map(blockForUpdate, block);
            }

            await _repo.SaveAll();
            var blockUpdatedList = _mapper.Map<List<BlocksForDetailedDto>>(user.Blocks);
            return Ok(blockUpdatedList);
        }

        [HttpPost("addNewItems/{userId}")]
        public async Task<IActionResult> AddNewItems(int userId, [FromBody] List<BlocksForNewDetailedDto> blocksForNewDetailedDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            List<Block> blocks = _mapper.Map<List<Block>>(blocksForNewDetailedDto);
            User user = await _repo.GetUser(userId);
            blocks.ForEach(block =>
            {
                user.Blocks.Add(block);
            });

            await _repo.SaveAll();
            var blockUpdatedList = _mapper.Map<List<BlocksForDetailedDto>>(user.Blocks);
            return Ok(blockUpdatedList);
        }

        [HttpPut("{blockId}")]
        public async Task<IActionResult> UpdateItem(int blockId, BlockForUpdateDto blocksForUpdateDto)
        {
            Block block = await _repo.GetBlock(blockId);
            blocksForUpdateDto.Id = blockId;

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var updatedBlock = _mapper.Map(blocksForUpdateDto, block);
            await _repo.SaveAll();

            return Ok(updatedBlock);
        }

        [HttpGet("getScript/{blockId}")]
        public async Task<IActionResult> GetItemScript(int blockId)
        {
            Block block = await _repo.GetBlock(blockId);

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                using (var client = await _getConnectedSftpClient(block.User))
                {
                    MemoryStream stream = new MemoryStream();
                    client.DownloadFile(block.ScriptFileName, stream);
                    var script = Encoding.UTF8.GetString(stream.ToArray());
                    return Ok(script);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("uploadScript/{blockId}")]
        public async Task<IActionResult> uploadScriptFile(int blockId, [FromBody]string script)
        {
            Block block = await _repo.GetBlock(blockId);

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                if (block.ScriptFileName.Length == 0)
                {
                    block.ScriptFileName = block.UserId + "_" + block.Id + ".py";
                }

                using (var client = await _getConnectedSftpClient(block.User))
                {
                    Stream scriptStream = this._generateStreamFromString(script);

                    string scriptFileName = block.ScriptFileName;
                    bool isFileExist = false;

                    IEnumerable<string> files = client.ListDirectory("").Select(s => s.Name);

                    foreach (string fileName in files)
                    {
                        if (fileName == scriptFileName)
                        {
                            isFileExist = true;
                            break;
                        }
                    }

                    client.UploadFile(scriptStream, scriptFileName);
                    scriptStream.Close();
                    await _repo.SaveAll();

                    return Ok("File: " + scriptFileName + " Replaced: " + isFileExist);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Stream _generateStreamFromString(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private async Task<SftpClient> _getConnectedSftpClient(User user)
        {
            try
            {
                var sshStream = await _cloudStorage.downloadStreamFromBlobContainer(user.SshBlobName);
                var privateKeyFile = new PrivateKeyFile(sshStream);
                var client = new SftpClient(user.RaspHost, user.RaspUsername, privateKeyFile);
                client.Connect();
                return client;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}