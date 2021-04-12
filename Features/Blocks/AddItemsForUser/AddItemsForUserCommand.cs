using System.Collections.Generic;
using MediatR;
using SmartRoomsApp.API.Dtos;
using SmartRoomsApp.API.Features.Blocks.GetItem;

namespace SmartRoomsApp.API.Features.Blocks.AddItemsForUser
{
    public class AddItemsForUserCommand : IRequest<IEnumerable<GetItemResult>>
    {
        public List<BlocksForNewDetailedDto> blocks;
        public int UserId { get; set; }
    }
}