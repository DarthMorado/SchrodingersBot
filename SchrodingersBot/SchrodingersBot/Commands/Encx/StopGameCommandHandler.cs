using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class StopGameCommandHandler : IBotCommandHandler<stopgameCommand>
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _repository;

        public StopGameCommandHandler(IDbRepository<EncxGameSubscriptionEntity> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(stopgameCommand request, CancellationToken cancellationToken)
        {
            var activeGames = await _repository.FindAsync(x => x.ChatId == request.Message.ChatId && x.IsActive);
            if (activeGames is null || !activeGames.Any())
            {
                return null;
            }

            foreach (var game in activeGames)
            {
                game.IsActive = false;
                await _repository.UpdateAsync(game);
            }

            return null;
        }
    }
}
