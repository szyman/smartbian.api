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
        }
    }
}