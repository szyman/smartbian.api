using System.Collections.Generic;
using MediatR;
using SmartRoomsApp.API.Features.Blocks.GetItem;

namespace SmartRoomsApp.API.Features.Blocks.GetItemsForUser
{
    public class GetItemsForUserQuery : IRequest<IEnumerable<GetItemResult>>
    {
        public int UserId { get; set; }
    }
}