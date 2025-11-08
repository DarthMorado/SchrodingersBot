using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NotABot.Wrapper.Answer;

namespace SchrodingersBot.Commands
{
    public class GameCommandHandler : IBotCommandHandler<gameCommand>
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptions;

        public GameCommandHandler(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptions)
        {
            _gameSubscriptions = gameSubscriptions;
        }

        public async Task<Result> Handle(gameCommand request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.ChatId;
            var activeSubscriptions = await _gameSubscriptions.FindAsync(x => x.ChatId == chatId && x.IsActive);

            if (activeSubscriptions is null || !activeSubscriptions.Any())
            {
                return new Result()
                {
                    new Answer()
                    {
                        ChatId = chatId,
                        AnswerType = AnswerTypes.Text,
                        IsHtml = false,
                        Text = "No active Games"
                    }
                };
            }

            return Result.SimpleText(request.Message, $"{activeSubscriptions.First().Domain}/{activeSubscriptions.First().GameId}");
        }
    }
}
