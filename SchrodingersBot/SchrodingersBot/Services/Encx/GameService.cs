using AutoMapper;
using Azure.Core;
using HtmlAgilityPack;
using MediatR;
using Microsoft.Identity.Client;
using NotABot.Wrapper;
using PuppeteerSharp;
using SchrodingersBot.Commands;
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
        private readonly IMediator _mediator;

        public GameService(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IMapper mapper,
            IMediator mediator
            )
        {
            _mediator = mediator;
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
        }

        public async Task<Result> FormatGameState(IncomingMessage message, EncxGameEngineModel game, bool needScreenshot = false)
        {
            Result result = new();

            var lvl = game.Level;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Уровень {lvl.Number}/{game.Levels.Count} {lvl.Name}");
            if (lvl.Bonuses != null && lvl.Bonuses.Any())
            {
                sb.AppendLine($"На уровне бонусы. ({lvl.Bonuses.Count})");
                foreach (var bonus in lvl.Bonuses.OrderBy(x => x.Number))
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

            result.Add(Answer.SimpleText(message, sb.ToString()));

            sb = new StringBuilder();
            if (lvl.Task != null || lvl.Tasks != null)
            {
                sb.AppendLine($"Задание:");
                if (!String.IsNullOrWhiteSpace(lvl?.Task?.TaskText))
                {
                    sb.AppendLine(lvl.Task.TaskText);
                }
                foreach (var task in lvl.Tasks ?? new())
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml($"<html><body>{task.TaskText}</body></html>");
                    var txt = doc.DocumentNode.InnerText.Trim();
                    if (!String.IsNullOrWhiteSpace(txt))
                    {
                        sb.AppendLine(txt);
                    }
                }
                if (sb.Length > 0)
                {
                    result.Add(Answer.SimpleText(message, sb.ToString(), false));
                }
            }

            if (needScreenshot)
            {
                result.AddRange(await _mediator.Send(new screenshotCommand() { Message = message }));
            }

            return result;
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
