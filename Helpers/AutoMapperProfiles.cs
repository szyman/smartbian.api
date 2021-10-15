using System.Collections.Generic;
using AutoMapper;
using SmartRoomsApp.API.Dtos;
using SmartRoomsApp.API.Features.Blocks.GetItem;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>();
            CreateMap<User, UserForDetailedDto>();
            CreateMap<Block, GetItemResult>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<BlocksForDetailedDto, Block>();
            CreateMap<Block, BlocksForDetailedDto>();
            CreateMap<BlockForUpdateDto, Block>();
            CreateMap<BlocksForNewDetailedDto, Block>();
            CreateMap<UserForRegisterDto, User>();
        }
    }
}