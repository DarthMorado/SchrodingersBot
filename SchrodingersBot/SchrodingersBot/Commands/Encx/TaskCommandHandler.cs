using AutoMapper;
using HtmlAgilityPack;
using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SchrodingersBot.Commands
{
    public class TaskCommandHandler : IBotCommandHandler<taskCommand>
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionRepository;
        private readonly IDbRepository<EncxAuthEntity> _loginInfoRepository;
        private readonly IEncxEngine _engine;
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public TaskCommandHandler(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IEncxEngine engine,
            IGameService gameService,
            IMapper mapper)
        {
            _gameService = gameService;
            _engine = engine;
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
        }

        public async Task<Result> Handle(taskCommand request, CancellationToken cancellationToken)
        {
            var result = new Result();
            var activeGames = await _gameSubscriptionRepository.FindAsync(x => x.ChatId == request.Message.ChatId && x.IsActive);

            if (activeGames is null || !activeGames.Any())
            {
                return null;
            }

            var activeGame = activeGames.First();
            if (!activeGame.LoginInfoId.HasValue)
            {
                return null;
            }
            var loginInfoEntity = await _loginInfoRepository.GetByIdAsync(activeGame.LoginInfoId.Value);

            loginInfoEntity.Domain = activeGame.Domain;
            loginInfoEntity.GameId = activeGame.GameId;

            //var game = await _encxService.GetGameAsync(request.Message.ChatId, activeGame.Domain, activeGame.GameId, loginInfo);

            var game = await _engine.GetGameObject(loginInfoEntity);

            if (game is null)
            {
                return null;
            }
            
            return await _gameService.FormatGameState(request.Message, game, true);
        }
    }
}
