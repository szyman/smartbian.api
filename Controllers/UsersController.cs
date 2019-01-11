using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Dtos;
using System;
using SmartRoomsApp.API.Helpers;

namespace SmartRoomsApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudStorageRepository _cloudStorage;

        public UsersController(ICombiningRepository repo, IMapper mapper, ICloudStorageRepository cloudStorage)
        {
            _mapper = mapper;
            _repo = repo;
            _cloudStorage = cloudStorage;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);
            var updatedUser = _mapper.Map(userForUpdateDto, userFromRepo);
            var userToReturn = _mapper.Map<UserForDetailedDto>(updatedUser);

            if (await _repo.SaveAll())
                return Ok(userToReturn);

            throw new System.Exception($"Updating user {id} failed on save");
        }

        [HttpGet("{id}/getSshKey")]
        public async Task<IActionResult> GetSsh(int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            try
            {
                string sshKey = await _cloudStorage.downloadTextFromBlobContainer(userFromRepo.SshBlobName);
                return Ok(sshKey);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/saveSshKey")]
        public async Task<IActionResult> SaveSshKey(int id, [FromBody] string sshKey)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);
            userFromRepo.SshBlobName = id + "_rsa";

            try
            {
                string blobFileName = await _cloudStorage.uploadTextToBlobContainer(userFromRepo.SshBlobName, sshKey);
                await _repo.SaveAll();

                return Ok(blobFileName);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}