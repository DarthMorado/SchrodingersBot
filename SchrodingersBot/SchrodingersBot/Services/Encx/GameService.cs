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
using SchrodingersBot.DTO;
using SchrodingersBot.DTO.Encx;
using SchrodingersBot.DTO.EnGame;
using SchrodingersBot.Services.Text;
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
        private readonly IHtmlProcessingService _htmlProcessingService;

        public GameService(IDbRepository<EncxGameSubscriptionEntity> gameSubscriptionRepository,
            IDbRepository<EncxAuthEntity> loginInfoRepository,
            IMapper mapper,
            IMediator mediator,
            IHtmlProcessingService htmlProcessingService
            )
        {
            _htmlProcessingService = htmlProcessingService;
            _mediator = mediator;
            _gameSubscriptionRepository = gameSubscriptionRepository;
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
        }

        public async Task<Result> FormatGameState(IncomingMessage message, EncxGameEngineModel game, bool needScreenshot = false)
        {
            Result result = new();

            var additionalObjects = new List<MessageObjectDTO>();
            var newAdditionalObjects = new List<MessageObjectDTO>();

            var lvl = game.Level;
            if (lvl is null) return result;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(FormatLevelHeader(game));

            additionalObjects = await FormatLevelTask(game);
            if (additionalObjects.Any())
            {
                sb.AppendLine(additionalObjects[0].Text);
            }
            additionalObjects.AddRange(newAdditionalObjects.Skip(1));

            sb.AppendLine(await FormatLevelHelps(game, additionalObjects));
            sb.AppendLine(await FormatLevelBonuses(game));

            result.Add(Answer.SimpleText(message, sb.ToString(), true));

            foreach (var additionalObject in additionalObjects)
            {
                if (additionalObject.IsImage)
                {
                    result.Add(Answer.SimpleImage(message, additionalObject.Content, additionalObject.Text));
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
                newLine = $"🔦: <b>{lvl.SectorsLeftToClose}</b> ({lvl.RequiredSectorsCount}/{lvl.Sectors?.Count ?? 0}) | ";
                if (lvl.Timeout == 0)
                {
                    newLine += "⏳: -- |";
                }
                else
                {
                    newLine += $"⏳: {ConvertTimeFromSeconds(lvl.Timeout)} |";
                }

                if (lvl.Helps != null && lvl.Helps.Any())
                {
                    newLine += $"💡: {lvl.Helps.Count} ";

                    var remainingTimes = lvl.Helps.Where(x => x.RemainSeconds > 0).Select(x => x.RemainSeconds);
                    if (remainingTimes.Any())
                    {
                        newLine += $"({ConvertTimeFromSeconds(remainingTimes.Min())}) ";
                    }

                    newLine += "|";
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

        private async Task<List<MessageObjectDTO>> FormatLevelTask(EncxGameEngineModel game)
        {
            var result = new List<MessageObjectDTO>();
            var additionalObjects = new List<MessageObjectDTO>();
            try
            {
                var lvl = game.Level;
                if (lvl == null) return result;

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

                foreach (var task in tasks)
                {
                    //sb.AppendLine(EscapeHtml(task.TaskText, out _));
                    var objects = await _htmlProcessingService.PrepareHtmlForTgAsync(task.TaskText);
                    if (objects.Any())
                    {
                        sb.AppendLine(objects[0].Text);
                        additionalObjects.AddRange(objects.Skip(1));
                    }

                }

                result.Add(new MessageObjectDTO()
                {
                    Text = sb.ToString(),
                });
                result.AddRange(additionalObjects);
                return result;
            }
            catch
            {
                return result;
            }
        }
        private async Task<string> FormatLevelHelps(EncxGameEngineModel game, List<MessageObjectDTO> additionalObjects)
        {
            try
            {
                var lvl = game.Level;
                if (lvl is null || lvl.Helps is null || !lvl.Helps.Any()) return string.Empty;

                StringBuilder sb = new();

                foreach (var help in lvl.Helps.Where(x => !x.IsPenalty).OrderBy(x => x.Number))
                {
                    sb.Append($"💡 <b>Подсказка {help.Number}</b>:");
                    if (!String.IsNullOrWhiteSpace(help.HelpText))
                    {
                        sb.AppendLine();
                        var objects = await _htmlProcessingService.PrepareHtmlForTgAsync(help.HelpText);
                        if (objects.Any())
                        {
                            sb.AppendLine(objects[0].Text);
                            additionalObjects.AddRange(objects.Skip(1)); //todo
                        }
                    }
                    else if (help.RemainSeconds != 0)
                    {
                        sb.AppendLine($" <i>будет доступна через {ConvertTimeFromSeconds(help.RemainSeconds)}</i>");
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

        private async Task<string> FormatLevelBonuses(EncxGameEngineModel game)
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
                        var objects = await _htmlProcessingService.PrepareHtmlForTgAsync(bonus.Task);
                        if (objects.Any())
                        {
                            sb.AppendLine($"<i>{objects[0].Text}</i>");
                            //additionalObjects.AddRange(objects.Skip(1)); //todo
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(bonus.Help))
                    {
                        var objects = await _htmlProcessingService.PrepareHtmlForTgAsync(bonus.Help);
                        if (objects.Any())
                        {
                            sb.AppendLine($"{objects[0].Text}");
                            //additionalObjects.AddRange(objects.Skip(1)); //todo
                        }
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
            int minutes = ((time / 60) % 60);
            if (minutes != 0)
            {
                sb.Append($"{minutes}м ");
            }
            var seconds = time % 60;
            if (seconds != 0)
            {
                sb.Append($"{seconds}c ");
            }
            return sb.ToString();
        }

        //private string EscapeHtml(string input, out List<object> additionalObjects)
        //{
        //    string result = input
        //        .Replace("&", "&amp;")
        //        .Replace("<", "&lt;")
        //        .Replace(">", "&gt;")
        //        .Replace("\"", "&quot;");

        //    additionalObjects = new();
        //    return result;
        //}
    }
}
