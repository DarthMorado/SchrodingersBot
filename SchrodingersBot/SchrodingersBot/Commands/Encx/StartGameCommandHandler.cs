using NotABot.Wrapper;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class StartGameCommandHandler : IBotCommandHandler<startgameCommand>
    {
        private readonly IEncxService _encxService;

        public StartGameCommandHandler(IEncxService encxService)
        {
            _encxService = encxService;
        }

        public async Task<List<Answer>> Handle(startgameCommand request, CancellationToken cancellationToken)
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

            return new List<Answer>()
            {
                new()
                {
                    ChatId = request.Message.ChatId,
                    Text = (gameInfo is null) ? "Something went wrong" : $"{gameInfo.GameId}",
                    AnswerType = "message",
                    IsHtml = false
                }
            };
        }
    }
}
