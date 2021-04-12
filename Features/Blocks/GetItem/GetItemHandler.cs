using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Features.Blocks.GetItem
{
    public class GetItemHandler : IRequestHandler<GetItemQuery, GetItemResult>
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _mapper;
        public GetItemHandler(ICombiningRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<GetItemResult> Handle(GetItemQuery request, CancellationToken cancellationToken)
        {
            Block block = await _repo.GetBlock(request.BlockId);
            var result = _mapper.Map<GetItemResult>(block);
            return result;
        }
    }
}