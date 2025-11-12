using AutoMapper;
using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SchrodingersBot.Commands
{
    public class TaskCommandHandler : IBotCommandHandler<taskCommand>
    {
        private readonly IDbRepository<EncxGameSubscriptionEntity> _gameSubscriptionRepository;
        private readonly IDbRepository<EncxAuthEntity> _loginInfoRepository;
        private readonly IEncxService _encxService;
        private readonly IEncxEngine _engine;
        private readonly IMapper _mapper;

        public TaskCommandHandler(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IEncxService encxService,
            IEncxEngine engine,
            IMapper mapper)
        {
            _engine = engine;
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _encxService = encxService;
            _mapper = mapper;
        }

        public async Task<Result> Handle(taskCommand request, CancellationToken cancellationToken)
        {
            var result = new Result();
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


            loginInfoEntity.Domain = activeGame.Domain;
            loginInfoEntity.GameId = activeGame.GameId;

            //var game = await _encxService.GetGameAsync(request.Message.ChatId, activeGame.Domain, activeGame.GameId, loginInfo);

            var game = await _engine.GetGameObject(loginInfoEntity);

            var lvl = game.Level;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Уровень {lvl.Number}/{game.Levels.Count} {lvl.Name}");
            if (lvl.Bonuses != null && lvl.Bonuses.Any())
            {
                sb.AppendLine($"На уровне бонусы. ({lvl.Bonuses.Count})");
                foreach(var bonus in lvl.Bonuses.OrderBy(x => x.Number))
                {
                    sb.AppendLine($"{bonus.Number}: {bonus.Name} ({(bonus.Negative ? "-" : "")}{bonus.AwardTime}с)");
                    if (!String.IsNullOrWhiteSpace(bonus.Task))
                    {
                        sb.AppendLine(bonus.Task);
                    }
                    
                }
            }
            sb.AppendLine($"Секторов для закрытия:{lvl.RequiredSectorsCount}");
            if (lvl.Sectors != null)
            {
                foreach (var sector in lvl.Sectors.OrderBy(x => x.Order))
                {
                    sb.AppendLine($"{sector.Order}: {sector.Name} ({sector.Answer})"); //todo
                }
            }

            result.Add(Answer.SimpleText(request.Message, sb.ToString()));
            
            sb = new StringBuilder();
            if (lvl.Task != null || lvl.Tasks != null)
            {
                sb.AppendLine($"Задание:");
                if (!String.IsNullOrWhiteSpace(lvl?.Task?.TaskText))
                {
                    sb.AppendLine(lvl.Task.TaskText);
                }
                foreach(var task in lvl.Tasks ?? new())
                {
                    sb.AppendLine(task.TaskText);
                }
                result.Add(Answer.SimpleText(request.Message, sb.ToString(), true));
            }

                return result;
        }
    }
}
