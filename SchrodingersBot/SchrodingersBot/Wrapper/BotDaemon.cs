using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace NotABot.Wrapper
{
    public abstract class BotDaemon<T> : IHostedService
        where T : BotOptions
    {
        private readonly BotOptions _options;
        private readonly TelegramBotClient _bot;
        private CancellationTokenSource _cancellationTokenSource;
        public bool Enabled { get; set; }
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
        private List<long> SubscribedChats { get; set; }
        private DateTime LastRun { get; set; }

        public abstract Task<Result> Action();
        public abstract Task Subscribe(long chatId, Dictionary<string, object> options);
        public abstract Task UnSubscribe(long chatId, Dictionary<string, object> options);
        public abstract Task ProcessUnexpectedError(Exception ex);

        protected BotDaemon(T options)
        {
            _options = options;
            _bot = new TelegramBotClient(options.ApiToken);
            _cancellationTokenSource = new();
        }

        public async Task ChangeInterval(TimeSpan interval)
        {
            Interval = interval;
        }

        public async Task ChangeEnabledStatus(bool enabled)
        {
            Enabled = enabled;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var results = await Action();

                    var answers = (List<Answer>)results;
                    foreach (var answer in answers)
                    {
                        PrepareAnswer(answer);

                        if (answer.AnswerType == Answer.AnswerTypes.Text)
                        {
                            await _bot.SendMessage(chatId: answer.ChatId,
                                linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = answer.DisableWebPagePreview },
                                parseMode: answer.IsHtml ? ParseMode.Html : ParseMode.MarkdownV2,
                                text: answer.Text.ToString(CultureInfo.CreateSpecificCulture("en-GB")),
                                replyParameters: answer.ReplyToMessageId.HasValue ? new ReplyParameters() { MessageId = answer.ReplyToMessageId.Value } : null,
                                cancellationToken: new CancellationTokenSource(1000).Token
                                );
                        }
                        if (answer.AnswerType == Answer.AnswerTypes.Reaction)
                        {
                            await _bot.SetMessageReaction(chatId: answer.ChatId,
                                messageId: answer.ReplyToMessageId.Value,
                                reaction: new List<ReactionTypeEmoji>()
                                {
                                    new ()
                                    {
                                        Emoji = answer.Text
                                    }
                                }
                                );
                        }
                        if (answer.AnswerType == Answer.AnswerTypes.Image)
                        {
                            using var ms = new MemoryStream(answer.Content);
                            await _bot.SendPhoto(chatId: answer.ChatId, new InputFileStream(ms, answer.Text));
                        }
                    }

                    await Task.Delay(Interval);
                }
                catch(Exception ex)
                {
                    await ProcessUnexpectedError(ex);
                }
            }
        }

        private void PrepareAnswer(Answer input)
        {
            var specialSymbols = new List<string>()
            {
                "\\",
                "_",
                "*",
                "[",
                "]",
                "(",
                ")",
                "~",
                "`",
                ">",
                "<",
                "&",
                "#",
                "+",
                "-",
                "=",
                "|",
                "{",
                "}",
                ".",
                "!"
            };

            if (!input.IsHtml && !String.IsNullOrWhiteSpace(input.Text))
            {
                foreach (string specialSymbol in specialSymbols)
                {
                    input.Text = input.Text.Replace(specialSymbol, $"\\{specialSymbol}");
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Run(_cancellationTokenSource.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
