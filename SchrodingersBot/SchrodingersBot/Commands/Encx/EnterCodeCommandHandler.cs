using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.Encx;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SchrodingersBot.Commands
{
    public class EnterCodeCommandHandler : IBotCommandHandler<entercodeCommand>
    {
        private readonly IGameService _gameService;
        private readonly IEncxEngine _encxEngine;
        private readonly IDbRepository<EncxGameSubscriptionEntity> _subscriptionsRepository;
        private readonly IDbRepository<ChatParameterEntity> _chatParametersRepository;

        public EnterCodeCommandHandler(IGameService gameService,
            IEncxEngine encxEngine,
            IDbRepository<EncxGameSubscriptionEntity> subscriptionsRepository,
            IDbRepository<ChatParameterEntity> chatParametersRepository
            )
        {
            _chatParametersRepository = chatParametersRepository;
            _subscriptionsRepository = subscriptionsRepository;
            _gameService = gameService;
            _encxEngine = encxEngine;
        }

        public async Task<Result> Handle(entercodeCommand request, CancellationToken cancellationToken)
        {
            bool? isCorrect = null;
            
            var game = await _gameService.GetActiveGame(request.Message.ChatId);

            if (game is null)
            {
                return null;
            }

            var prefixData = await _chatParametersRepository.FindAsync(x => x.ChatId == request.Message.ChatId && x.Code == "PREFIX");
            string prefix = null;
            if (prefixData != null && prefixData.Any())
            {
                prefix = prefixData.First().TextValue;
            }

            var gameState = await _encxEngine.EnterCode(game.LoginInfo, game.ActiveLevelId, game.ActiveLevelNumber, request.Message.Parameter);
            if (!(gameState?.EngineAction?.LevelAction?.IsCorrectAnswer ?? false) &&
                !(gameState?.EngineAction?.BonusAction?.IsCorrectAnswer ?? false) &&
                !String.IsNullOrWhiteSpace(prefix) &&
                !request.Message.Parameter.StartsWith(prefix)
                )
            {
                var newCode = $"{prefix}{request.Message.Parameter}";
                gameState = await _encxEngine.EnterCode(game.LoginInfo, game.ActiveLevelId, game.ActiveLevelNumber, newCode);
            }

            Answer.ReactionType resultType = Answer.ReactionType.Unknown;

            if (!string.IsNullOrEmpty(gameState?.EngineAction?.LevelAction?.Answer))
            {
                isCorrect = gameState?.EngineAction?.LevelAction?.IsCorrectAnswer ?? false;
                resultType = isCorrect switch
                {
                    true => Answer.ReactionType.Heart,
                    false => Answer.ReactionType.Shit,
                };
            }
            else if (!string.IsNullOrEmpty(gameState?.EngineAction?.BonusAction?.Answer))
            {
                isCorrect = gameState?.EngineAction?.BonusAction?.IsCorrectAnswer ?? false;
                resultType = isCorrect switch
                {
                    true => Answer.ReactionType.Celebration,
                    false => Answer.ReactionType.Shit,
                };
            }

            var result = Result.Reaction(request.Message, resultType);

            if (gameState.Level.LevelId != game.ActiveLevelId)
            {
                game = await _subscriptionsRepository.GetByIdAsync(game.Id);
                game.ActiveLevelId = gameState.Level.LevelId;
                game.ActiveLevelNumber = gameState.Level.Number;
                await _subscriptionsRepository.UpdateAsync(game);

                result.AddRange(await _gameService.FormatGameState(request.Message, gameState, true));
            }

            return result;
        }

        private bool CheckIfCanSendCode(EncxGameEngineModel? game)
        {
            if (game == null) return false;

            if (game.Level.IsPassed == true ||
                game.Level.Dismissed == true ||
                !(game.Level.HasAnswerBlockRule == false || game.Level.BlockDuration <= 0))
            {
                return false;
            }

            return true;
        }
    }
}
