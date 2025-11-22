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
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionsRepository;
        private readonly IDbRepository<EncxAuthEntity> _authRepository;
        private readonly IEncxEngine _engine;

        public StartGameCommandHandler(
            IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionsRepository,
            IDbRepository<EncxAuthEntity> authRepository,
            IEncxEngine engine)
        {
            _gameSubscriptionsRepository = gameSubscriptionsRepository;
            _engine = engine;
            _authRepository = authRepository;
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

            EncxAuthEntity? loginInfo = new();
            string url;

            url = request.Message.Parameters[0];
            var uri = new Uri(url);
            loginInfo.Domain = uri.Host;

            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                loginInfo.GameId = uri.Query.Split(new char[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=')).Where(x => x[0].ToLower() == "gid").First()[1];
            }
            else if (uri.Segments.Any(x => x.ToLower() == "play/"))
            {
                var gameIdStr = uri.Segments.SkipWhile(x => x.ToLower() != "play/").Skip(1).FirstOrDefault().Trim('\\').Trim('/');
                if (!String.IsNullOrEmpty(gameIdStr))
                {
                    if (int.TryParse(gameIdStr, out _))
                    {
                        loginInfo.GameId = gameIdStr;
                    }
                }
            }

            if (string.IsNullOrEmpty(loginInfo.Domain) || string.IsNullOrEmpty(loginInfo.GameId))
            {
                return null;
            }

            switch (request.Message.Parameters.Count)
            {
                case 3:
                    // Start game with user/pass
                    //loginInfo = await _encxService.GetLoginInfo(request.Message.ChatId, domain, gameId, , request.Message.Parameters[2]);
                    loginInfo.Username = request.Message.Parameters[1];
                    loginInfo.Password = request.Message.Parameters[2];
                    loginInfo = await _engine.Login(loginInfo);
                    break;
                default:
                    return null;
            }

            if (String.IsNullOrEmpty(loginInfo?.BrowserCookiesJson))
            {
                return null;
            }
            else
            {
                await _authRepository.CreateAsync(loginInfo);
            }

            var gameState = await _engine.GetGameObject(loginInfo);
            //var gameInfo = await _encxService.GetGameAsync(request.Message.ChatId, url, loginInfo);

            var gameSubEntity = new EncxGameSubscriptionEntity()
            {
                ChatId = request.Message.ChatId,
                Domain = loginInfo.Domain,
                GameId = loginInfo.GameId,
                LoginInfoId = loginInfo.Id,
                IsActive = true,
                ActiveLevelId = gameState?.Level?.LevelId ?? 0,
                ActiveLevelNumber = gameState?.Level?.Number ?? 0,
            };

            await _gameSubscriptionsRepository.CreateAsync(gameSubEntity);


            return new Result()
            {
                new()
                {
                    ChatId = request.Message.ChatId,
                    Text = (gameState is null) ? "Something went wrong" : $"{gameState.GameId}",
                    AnswerType = AnswerTypes.Text,
                    IsHtml = false
                }
            };
        }
    }
}
