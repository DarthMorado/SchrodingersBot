using NotABot.Wrapper;
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

        public EnterCodeCommandHandler(IGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task<Result> Handle(entercodeCommand request, CancellationToken cancellationToken)
        {
            bool? isCorrect = null;

            var game = await _gameService.GetActiveGame(request.Message.ChatId);
            if (!CheckIfCanSendCode(game))
            {
                return Result.Reaction(request.Message, Answer.ReactionType.Unknown);
            }

            isCorrect = await _gameService.EnterCode(request.Message.ChatId, request.Message.Parameter);

            return Result.Reaction(request.Message, isCorrect switch
            {
                true => Answer.ReactionType.Heart,
                false => Answer.ReactionType.Shit,
                _ => Answer.ReactionType.Unknown
            });
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
