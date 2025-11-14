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

namespace SchrodingersBot.Commands
{
    public class EnterCodeCommandHandler : IBotCommandHandler<entercodeCommand>
    {
        private readonly IGameService _gameService;
        private readonly IEncxEngine _encxEngine;
        private readonly IDbRepository<EncxGameSubscriptionEntity> _subscriptionsRepository;

        public EnterCodeCommandHandler(IGameService gameService,
            IEncxEngine encxEngine,
            IDbRepository<EncxGameSubscriptionEntity> subscriptionsRepository
            )
        {
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

            var gameState = await _encxEngine.EnterCode(game.LoginInfo, game.ActiveLevelId, game.ActiveLevelNumber, request.Message.Parameter);

            isCorrect = gameState?.EngineAction?.LevelAction?.IsCorrectAnswer ??
                        gameState?.EngineAction?.BonusAction?.IsCorrectAnswer;

            var result = Result.Reaction(request.Message, isCorrect switch
            {
                true => Answer.ReactionType.Heart,
                false => Answer.ReactionType.Shit,
                _ => Answer.ReactionType.Unknown
            });

            if (gameState.Level.LevelId != game.ActiveLevelId)
            {
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
