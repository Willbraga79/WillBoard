﻿using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WillBoard.Core.Consts;
using WillBoard.Core.Errors;
using WillBoard.Core.Interfaces.Caches;
using WillBoard.Core.Interfaces.Repositories;
using WillBoard.Core.Managers;
using WillBoard.Core.Results;

namespace WillBoard.Application.Administration.Commands.BoardThreadPin
{
    public class BoardThreadPinCommandHandler : IRequestHandler<BoardThreadPinCommand, Status<InternalError>>
    {
        private readonly AccountManager _accountManager;
        private readonly IPostRepository _postRepository;
        private readonly IBoardCache _boardCache;
        private readonly IPostCache _postCache;

        public BoardThreadPinCommandHandler(AccountManager accountManager, IPostRepository postRepository, IBoardCache boardCache, IPostCache postCache)
        {
            _accountManager = accountManager;
            _postRepository = postRepository;
            _boardCache = boardCache;
            _postCache = postCache;
        }

        public async Task<Status<InternalError>> Handle(BoardThreadPinCommand request, CancellationToken cancellationToken)
        {
            var board = await _boardCache.GetAsync(request.BoardId);

            if (board == null)
            {
                return Status<InternalError>.ErrorStatus(new InternalError(404, TranslationKey.ErrorNotFound));
            }

            if (!_accountManager.CheckPermission(request.BoardId, e => e.PermissionThreadPin))
            {
                return Status<InternalError>.ErrorStatus(new InternalError(403, TranslationKey.ErrorForbidden));
            }

            var post = await _postCache.GetAdaptedAsync(board, request.PostId);

            if (post == null || post.ThreadId != null)
            {
                return Status<InternalError>.ErrorStatus(new InternalError(404, TranslationKey.ErrorNotFound));
            }

            await _postRepository.UpdatePinAsync(post.BoardId, post.PostId, request.Pin);

            await _postCache.PurgeAdaptedCollectionAsync(board.BoardId);

            return Status<InternalError>.SuccessStatus();
        }
    }
}