using MediatR;
using Microsoft.Extensions.Options;
using NotABot.Wrapper;
using SchrodingersBot.Commands;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Daemons
{
    public class EncxGameUpdateDaemon : BotDaemon<GolfBotOptions>
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionsRepository;
        private readonly IGameService _gameService;
        private readonly IMediator _mediator;

        public EncxGameUpdateDaemon(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionsRepository,
            IGameService gameService,
            IMediator mediator,
            IOptions<GolfBotOptions> options)
            : base(options.Value)
        {
            _gameSubscriptionsRepository = gameSubscriptionsRepository;
            _gameService = gameService;
            _mediator = mediator;
        }

        public async override Task<Result> Action()
        {
            var result = new Result();

            var activeSubscriptions = await _gameSubscriptionsRepository.FindAsync(x => x.IsActive);
            foreach (var activeSubscription in activeSubscriptions)
            {
                var activeGame = await _gameService.GetActiveGame(activeSubscription.ChatId);
                if (activeGame?.Level is null)
                {
                    continue;
                }
                if (activeGame.Level.Number <= activeSubscription.ActiveLevelNumber)
                {
                    continue;
                }
                activeSubscription.ActiveLevelId = activeGame.Level.LevelId;
                activeSubscription.ActiveLevelNumber = activeGame.Level.Number;
                await _gameSubscriptionsRepository.UpdateAsync(activeSubscription);

                var task = await _mediator.Send(new taskCommand()
                {
                    Message = new IncomingMessage()
                    {
                        ChatId = activeSubscription.ChatId
                    }
                });
                if (task != null && task.Any())
                {
                    result.AddRange(task);
                }

                var screenshot = await _mediator.Send(new screenshotCommand()
                {
                    Message = new()
                    {
                        ChatId = activeSubscription.ChatId,
                    }
                });
                if (screenshot != null && screenshot.Any())
                {
                    result.AddRange(screenshot);
                }
            }

            return result;
        }

        public async override Task ProcessUnexpectedError(Exception ex)
        {
            
        }

        public async override Task Subscribe(long chatId, Dictionary<string, object> options)
        {
            
        }

        public async override Task UnSubscribe(long chatId, Dictionary<string, object> options)
        {
            
        }
    }
}
