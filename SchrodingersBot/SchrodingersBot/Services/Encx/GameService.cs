using AutoMapper;
using Azure.Core;
using HtmlAgilityPack;
using MediatR;
using Microsoft.Extensions.Primitives;
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
            if (lvl is null) return result;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(FormatLevelHeader(game));
            sb.AppendLine(FormatLevelTask(game));
            sb.AppendLine(FormatLevelHelps(game));
            sb.AppendLine(FormatLevelBonuses(game));
            //if (lvl.Bonuses != null && lvl.Bonuses.Any())
            //{
            //    sb.AppendLine($"На уровне бонусы. ({lvl.Bonuses.Count})");
            //    foreach (var bonus in lvl.Bonuses.OrderBy(x => x.Number))
            //    {
            //        sb.AppendLine($"{bonus.Number}: {bonus.Name} ({(bonus.Negative ? "-" : "")}{bonus.AwardTime}с)");
            //        if (!String.IsNullOrWhiteSpace(bonus.Task))
            //        {
            //            sb.AppendLine(bonus.Task);
            //        }

            //    }
            //}
            //sb.AppendLine($"Секторов для закрытия:{lvl.RequiredSectorsCount}");
            //if (lvl.Sectors != null)
            //{
            //    foreach (var sector in lvl.Sectors.OrderBy(x => x.Order))
            //    {
            //        sb.AppendLine($"{sector.Order}: {sector.Name} ({sector.Answer})"); //todo
            //    }
            //}

            result.Add(Answer.SimpleText(message, sb.ToString(), true));

            //sb = new StringBuilder();
            //if (lvl.Task != null || lvl.Tasks != null)
            //{
            //    sb.AppendLine($"Задание:");
            //    if (!String.IsNullOrWhiteSpace(lvl?.Task?.TaskText))
            //    {
            //        sb.AppendLine(lvl.Task.TaskText);
            //    }
            //    foreach (var task in lvl.Tasks ?? new())
            //    {
            //        HtmlDocument doc = new HtmlDocument();
            //        doc.LoadHtml($"<html><body>{task.TaskText}</body></html>");
            //        var txt = doc.DocumentNode.InnerText.Trim();
            //        if (!String.IsNullOrWhiteSpace(txt))
            //        {
            //            sb.AppendLine(txt);
            //        }
            //    }
            //    if (sb.Length > 0)
            //    {
            //        result.Add(Answer.SimpleText(message, sb.ToString(), false));
            //    }
            //}

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

        private string FormatLevelHeader(EncxGameEngineModel game)
        {
            try
            {
                var lvl = game.Level;
                if (lvl is null) return "";

                StringBuilder sb = new StringBuilder();
                string newLine = "";
                newLine = $"#EN{game.GameId} <b>Уровень {lvl.Number}/{game.Levels.Count}</b>";
                if (!String.IsNullOrWhiteSpace(lvl.Name))
                {
                    newLine += $": {lvl.Name}";
                }
                sb.AppendLine(newLine);
                newLine = $"🔦: {lvl.Sectors?.Count ?? 0} ({lvl.RequiredSectorsCount}) | ";
                if (lvl.Timeout == 0)
                {
                    newLine += "⏳: -- |";
                }
                else
                {
                    newLine += "⏳: ";
                    int hours = lvl.Timeout / 3600;
                    if (hours > 0)
                    {
                        newLine += $"{hours}ч ";
                    }
                    int minutes = (lvl.Timeout / 60) % 60;
                    if (minutes != 0)
                    {
                        newLine += $"{hours}м ";
                    }
                    var seconds = lvl.Timeout % 60;
                    if (seconds != 0)
                    {
                        newLine += $"{seconds}c ";
                    }
                    newLine += "|";
                }

                if (lvl.Helps != null && lvl.Helps.Any())
                {
                    newLine += $"💡: {lvl.Helps.Count} |";
                }

                if (lvl.Bonuses != null && lvl.Bonuses.Any())
                {
                    newLine += $"🎁: {lvl.Bonuses.Count} |";
                }
                sb.AppendLine(newLine);
                return sb.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        private string FormatLevelTask(EncxGameEngineModel game)
        {
            try
            {
                var lvl = game.Level;
                if (lvl == null) return String.Empty;

                List<EncxTask> tasks = new List<EncxTask>();

                if (lvl.Task != null)
                {
                    tasks.Add(lvl.Task);
                }
                if (lvl.Tasks != null && lvl.Tasks.Any())
                {
                    tasks.AddRange(lvl.Tasks);
                }

                StringBuilder sb = new();
                sb.AppendLine("<b>Задание:</b>");

                foreach(var task in tasks)
                {
                    sb.AppendLine(EscapeHtml(task.TaskText, out _));
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        private string FormatLevelHelps(EncxGameEngineModel game)
        {
            try
            {
                var lvl = game.Level;
                if (lvl is null || lvl.Helps is null || !lvl.Helps.Any()) return string.Empty;

                StringBuilder sb = new();

                foreach (var help in lvl.Helps.Where(x => !x.IsPenalty).OrderBy(x => x.Number))
                {
                    sb.AppendLine($"💡 <b>Подсказка {help.Number}</b>:");
                    if (!String.IsNullOrWhiteSpace(help.HelpText))
                    {
                        sb.AppendLine($"{EscapeHtml(help.HelpText, out _)}");
                    }
                    else if (help.RemainSeconds != 0)
                    {
                        sb.AppendLine($"<i>будет доступна через {ConvertTimeFromSeconds(help.RemainSeconds)}</i>");
                    }
                }

                foreach (var help in lvl.Helps.Where(x => x.IsPenalty).OrderBy(x => x.Number))
                {

                }

                    return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string FormatLevelBonuses(EncxGameEngineModel game)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                var lvl = game.Level;
                if (lvl is null || lvl.Bonuses is null || !lvl.Bonuses.Any()) return String.Empty;
                foreach (var bonus in lvl.Bonuses.OrderBy(x => x.Number))
                {
                    string awardTime = null;
                    if (bonus.AwardTime != 0)
                    {
                        if (bonus.Negative)
                        {
                            awardTime = $"штраф {ConvertTimeFromSeconds(bonus.AwardTime)}";
                        }
                        else
                        {
                            awardTime = $"бонус {ConvertTimeFromSeconds(bonus.AwardTime)}";
                        }
                    }
                    sb.AppendLine($"🎁{bonus.Number}: <b>{bonus.Name}</b>{(awardTime is null ? string.Empty : $" ({awardTime})")}:");
                    if (!String.IsNullOrWhiteSpace(bonus.Task))
                    {
                        sb.AppendLine($"<i>{EscapeHtml(bonus.Task, out _)}</i>");
                    }
                    if (!String.IsNullOrWhiteSpace(bonus.Help))
                    {
                        sb.AppendLine($"{EscapeHtml(bonus.Help, out _)}");
                    }

                }
                return sb.ToString();
            }
            catch
            {
                return String.Empty;
            }

        }

        private string ConvertTimeFromSeconds(int time)
        {
            if (time == 0) return string.Empty;

            StringBuilder sb = new();
            int hours = time / 3600;
            if (hours > 0)
            {
                sb.Append($"{hours}ч ");
            }
            int minutes = (time / 60) % 60;
            if (minutes != 0)
            {
                sb.Append($"{hours}м ");
            }
            var seconds = time % 60;
            if (seconds != 0)
            {
                sb.Append($"{seconds}c ");
            }
            return sb.ToString();
        }

        private string EscapeHtml(string input, out List<object> additionalObjects)
        {
            string result = input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");

            additionalObjects = new();
            return result;
        }
    }
}
