using System.Collections.Generic;
using AutoMapper;
using SmartRoomsApp.API.Dtos;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>();
            CreateMap<User, UserForDetailedDto>();
            CreateMap<Block, BlocksForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<BlocksForDetailedDto, Block>();
            CreateMap<BlockForUpdateDto, Block>();
            CreateMap<BlocksForNewDetailedDto, Block>();
            CreateMap<UserForRegisterDto, User>();
        }
    }
}