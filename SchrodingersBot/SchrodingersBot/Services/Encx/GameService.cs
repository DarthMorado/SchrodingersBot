using AutoMapper;
using Azure.Core;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.Encx;
using SchrodingersBot.DTO.EnGame;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Encx
{
    public interface IGameService
    {
        public Task<EncxGameEngineModel?> GetActiveGame(long chatId);
        public Task<bool?> EnterCode(long chatId, string code);
    }

    public class GameService : IGameService
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionRepository;
        private readonly IDbRepository<EncxAuthEntity> _loginInfoRepository;
        private readonly IMapper _mapper;
        private readonly IEncxService _encxService;

        public GameService(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IMapper mapper,
            IEncxService encxService)
        {
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
            _encxService = encxService;
        }

        public async Task<bool?> EnterCode(long chatId, string code)
        {
            var activeGames = await _gameSubscriptionRepository.FindAsync(x => x.ChatId == chatId && x.IsActive);

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

            var game = await _encxService.GetGameAsync(chatId, activeGame.Domain, activeGame.GameId, loginInfo);

            if (game.Level is null)
            {
                return null;
            }

            return await _encxService.EnterCode(activeGame.Domain, activeGame.GameId, game.Level.LevelId, game.Level.Number, loginInfo, code);
        }

        public async Task<EncxGameEngineModel?> GetActiveGame(long chatId)
        {
            var activeGames = await _gameSubscriptionRepository.FindAsync(x => x.ChatId == chatId && x.IsActive);

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

            var game = await _encxService.GetGameAsync(chatId, activeGame.Domain, activeGame.GameId, loginInfo);

            return game;
        }
    }
}
