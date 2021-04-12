using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Features.Blocks.GetItem;

namespace SmartRoomsApp.API.Features.Blocks.GetItemsForUser
{
    public class GetItemsForUserHandler : IRequestHandler<GetItemsForUserQuery, IEnumerable<GetItemResult>>
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _mapper;
        public GetItemsForUserHandler(ICombiningRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<IEnumerable<GetItemResult>> Handle(GetItemsForUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetUser(request.UserId);
            var result = _mapper.Map<IEnumerable<GetItemResult>>(user.Blocks);
            return result;
        }
    }
}