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
    public class GameService : IGameService
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionRepository;
        private readonly IDbRepository<EncxAuthEntity> _loginInfoRepository;
        private readonly IMapper _mapper;

        public GameService(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IMapper mapper
            )
        {
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
        }

        public async Task<EncxGameSubscriptionEntity?> GetActiveGame(long chatId)
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
            else
            {
                activeGame.LoginInfo = await _loginInfoRepository.GetByIdAsync(activeGame.LoginInfoId.Value);
                return activeGame;
            }
        }
    }
}
