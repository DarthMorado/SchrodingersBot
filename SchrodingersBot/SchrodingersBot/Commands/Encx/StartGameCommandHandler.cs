using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static NotABot.Wrapper.Answer;

namespace SchrodingersBot.Commands
{
    public class StartGameCommandHandler : IBotCommandHandler<startgameCommand>
    {
        private readonly IEncxService _encxService;
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionsRepository;

        public StartGameCommandHandler(IEncxService encxService,
            IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionsRepository)
        {
            _encxService = encxService;
            _gameSubscriptionsRepository = gameSubscriptionsRepository;
        }

        public async Task<Result> Handle(startgameCommand request, CancellationToken cancellationToken)
        {
            if (request?.Message?.Parameters is null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(request?.Message?.Parameter))
            {
                return null;
            }

            LoginInfoDTO loginInfo = null;
            string url;
            string domain = null;
            string gameId = null;

            url = request.Message.Parameters[0];
            var uri = new Uri(url);
            domain = uri.Host;

            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                gameId = uri.Query.Split(new char[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=')).Where(x => x[0].ToLower() == "gid").First()[1];
            }

            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(gameId))
            {
                return null;
            }

            switch (request.Message.Parameters.Count)
            {
                case 0:
                    return null;
                case 1:
                    // Start game without login
                    loginInfo = await _encxService.GetLoginInfo(request.Message.ChatId, domain);
                    //todo
                    break;
                case 2:
                    // Start game with known user
                    loginInfo = await _encxService.GetLoginInfo(request.Message.ChatId, domain, request.Message.Parameters[1]);
                    break;
                case 3:
                    // Start game with user/pass
                    loginInfo = await _encxService.GetLoginInfo(request.Message.ChatId, domain, request.Message.Parameters[1], request.Message.Parameters[2]);
                    break;
            }

            if (loginInfo is null)
            {
                return null;
            }

            var gameInfo = await _encxService.GetGameAsync(request.Message.ChatId, url, loginInfo);

            var gameSubEntity = new EncxGameSubscriptionEntity()
            {
                ChatId = request.Message.ChatId,
                Domain = domain,
                GameId = gameId,
                LoginInfoId = loginInfo.Id,
                IsActive = true,
                ActiveLevelId = gameInfo?.Level?.LevelId ?? 0,
                ActiveLevelNumber = gameInfo?.Level?.Number ?? 0,
            };

            await _gameSubscriptionsRepository.CreateAsync(gameSubEntity);


            return new Result()
            {
                new()
                {
                    ChatId = request.Message.ChatId,
                    Text = (gameInfo is null) ? "Something went wrong" : $"{gameInfo.GameId}",
                    AnswerType = AnswerTypes.Text,
                    IsHtml = false
                }
            };
        }
    }
}
