using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Dtos;

namespace SmartRoomsApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _imapper;
        public UsersController(ICombiningRepository repo, IMapper imapper)
        {
            _imapper = imapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();
            var usersToReturn = _imapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _imapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }
    }
}