using MediatR;

namespace SmartRoomsApp.API.Features.Blocks.GetItem
{
    public class GetItemQuery : IRequest<GetItemResult>
    {
        public int BlockId { get; set; }
    }
}