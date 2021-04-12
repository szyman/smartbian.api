using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SmartRoomsApp.API.Data;
using SmartRoomsApp.API.Features.Blocks.GetItem;
using SmartRoomsApp.API.Models;

namespace SmartRoomsApp.API.Features.Blocks.AddItemsForUser
{
    public class AddItemsForUserHandler : IRequestHandler<AddItemsForUserCommand, IEnumerable<GetItemResult>>
    {
        private readonly ICombiningRepository _repo;
        private readonly IMapper _mapper;
        public AddItemsForUserHandler(ICombiningRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        async Task<IEnumerable<GetItemResult>> IRequestHandler<AddItemsForUserCommand, IEnumerable<GetItemResult>>.Handle(AddItemsForUserCommand command, CancellationToken cancellationToken)
        {
            List<Block> blocks = _mapper.Map<List<Block>>(command.blocks);
            User user = await _repo.GetUser(command.UserId);
            blocks.ForEach(block =>
            {
                user.Blocks.Add(block);
            });

            await _repo.SaveAll();
            var blockUpdatedList = _mapper.Map<List<GetItemResult>>(user.Blocks);
            return blockUpdatedList;
        }
    }
}