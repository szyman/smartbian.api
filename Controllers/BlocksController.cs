using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public BlocksController(ICombiningRepository repo, IMapper mapper) {
            this._repo = repo;
            this._mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItems(int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(id);
            var blocksToReturn = _mapper.Map<IEnumerable<BlocksForDetailedDto>>(user.Blocks);

            return Ok(blocksToReturn);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> AddItems(int id, List<BlocksForDetailedDto> blocksForDetailed)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            List<Block> blocks = _mapper.Map<List<Block>>(blocksForDetailed);
            User user = await _repo.GetUser(id);

            //TODO: Always creating new items
            user.Blocks = blocks;
            await _repo.SaveAll();
            return StatusCode(201);
        }
    }
}