using AutoMapper;
using NotABot.Wrapper;
using PuppeteerSharp;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class ScreenshotCommandHandler : IBotCommandHandler<screenshotCommand>
    {
        private readonly IWebHelperService _webHelperService;
        private readonly IGameService _gameService;

        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionRepository;
        private readonly IDbRepository<EncxLoginInfoEntity> _loginInfoRepository;
        private readonly IMapper _mapper;
        private readonly IEncxService _encxService;

        public ScreenshotCommandHandler(IWebHelperService webHelperService,
            IGameService gameService,
            IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxLoginInfoEntity> loginInfoRepository,
            IMapper mapper,
            IEncxService encxService)
        {
            _webHelperService = webHelperService;
            _gameService = gameService;

            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
            _encxService = encxService;
        }

        private static string GameUrl(string domain, string gameId) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}";

        public async Task<Result> Handle(screenshotCommand request, CancellationToken cancellationToken)
        {
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
            var loginInfo = _mapper.Map<LoginInfoDTO>(loginInfoEntity);

            var url = GameUrl(activeGame.Domain, activeGame.GameId);

            List<CookieParam> cookies = new();
            cookies.Add(new CookieParam()
            {
                Name = "GUID",
                Value = loginInfo.Guid,
                Domain = $"{activeGame.Domain}",
            });
            cookies.Add(new CookieParam()
            {
                Name = "stoken",
                Value = loginInfo.Stoken,
                Domain = $".{activeGame.Domain}",
            });
            cookies.Add(new CookieParam()
            {
                Name = "atoken",
                Value = loginInfo.Atoken,
                Domain = ".en.cx",
            });
            //cookies.Add(new CookieParam()
            //{
            //    Name = "Domain",
            //    Value = activeGame.Domain,
            //    Domain = $".{activeGame.Domain}",
            //    Path = "/",
            //    Secure = false,
            //    HttpOnly = false,
            //    Expires = -1
            //});

            byte[] img = await _webHelperService.GetScreenshot(url, activeGame.Domain, loginInfo, cookies);

            return Result.SimpleImage(request.Message, img);
        }
    }
}
