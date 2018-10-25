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
        public BlocksController(ICombiningRepository repo, IMapper mapper)
        {
            this._repo = repo;
            this._mapper = mapper;
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
        public async Task<IActionResult> AddItems(int userId, List<BlocksForDetailedDto> blocksForDetailedDto)
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
        public async Task<IActionResult> AddNewItems(int userId, List<BlocksForNewDetailedDto> blocksForNewDetailedDto)
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
            PrivateKeyFile privateKeyFile;
            Block block = await _repo.GetBlock(blockId);

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (var client = new SftpClient(block.User.RaspHost, block.User.RaspUsername, privateKeyFile))
            {
                try
                {
                    MemoryStream stream = new MemoryStream();
                    client.Connect();
                    client.DownloadFile(block.ScriptFileName, stream);
                    var script = Encoding.UTF8.GetString(stream.ToArray());
                    return Ok(script);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPut("uploadScript/{blockId}")]
        public async Task<IActionResult> uploadScriptFile(int blockId, [FromBody]string script)
        {
            PrivateKeyFile privateKeyFile;
            Block block = await _repo.GetBlock(blockId);
            Stream scriptStream = this.generateStreamFromString(script);

            if (block.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                privateKeyFile = new PrivateKeyFile(@"C:\Users\Public\private_key");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (block.ScriptFileName.Length == 0)
            {
                block.ScriptFileName = block.UserId + "_" + block.Id + ".py";
                await _repo.SaveAll();
            }

            using (var client = new SftpClient(block.User.RaspHost, block.User.RaspUsername, privateKeyFile))
            {
                try
                {
                    string scriptFileName = block.ScriptFileName;
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

                    client.UploadFile(scriptStream, scriptFileName);
                    scriptStream.Close();

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

        private Stream generateStreamFromString(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}