using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Dtos;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration config, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {
            var userToCreate = new User
            {
                UserName = userForRegisterDto.Username
            };

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(userToCreate, "Member");
                return CreatedAtRoute("GetUser",
                    new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if (result.Succeeded)
            {
                var appUser = await _userManager.Users.Include(b => b.Blocks)
                    .FirstOrDefaultAsync(u => u.UserName == userForLoginDto.Username);

                return Ok(new
                {
                    token = _generateToken(appUser).Result
                    //TODO: Add appUser with mapped DTO eg. _mapper.Map<UserForListDto>(appuser)
                });
            }

            return Unauthorized();
        }

        [HttpPost("loginExtProvider")]
        public async Task<IActionResult> LoginExtProvider([FromBody] UserForLoginDto userForLoginDto)
        {
            var separator = "__";
            var fbUserName = userForLoginDto.Password + separator +  userForLoginDto.Username.Split(" ")[0];
            var password = userForLoginDto.Password + "f@cebook";
            var user = await _userManager.FindByNameAsync(fbUserName);
            if (user == null)
            {
                var userToCreate = new User
                {
                    UserName = fbUserName
                };

                var result = await _userManager.CreateAsync(userToCreate, password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                else
                {
                    await _userManager.AddToRoleAsync(userToCreate, "Member");
                }
            }
            else
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

                if (!result.Succeeded)
                {
                    return Unauthorized();
                }
            }

            var appUser = await _userManager.Users.Include(b => b.Blocks)
                        .FirstOrDefaultAsync(u => u.UserName == fbUserName);
            appUser.UserName = appUser.UserName.Split(separator)[1];

            return Ok(new
            {
                token = _generateToken(appUser).Result
                //TODO: Add appUser with mapped DTO eg. _mapper.Map<UserForListDto>(appuser)
            });
        }

        private async Task<string> _generateToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}