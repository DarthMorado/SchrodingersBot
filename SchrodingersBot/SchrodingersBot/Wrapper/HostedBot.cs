using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Threading;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Encodings.Web;
using System.Runtime.CompilerServices;

namespace NotABot.Wrapper
{
    public abstract class HostedBot<T> : IHostedBot<T>
       where T : BotOptions, new()
    {
        private CancellationTokenSource _cancellationToken;
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _bot;
        private readonly ReceiverOptions _receiverOptions;

        public HostedBot(IMediator mediator, T options)
        {
            _cancellationToken = new CancellationTokenSource();
            _mediator = mediator;
            _bot = new TelegramBotClient(options.ApiToken);
            _receiverOptions = new() { AllowedUpdates = { } };
        }

        public abstract IncomingMessage PrepareIncommingMessageCommandName(IncomingMessage input);
        public abstract Task ProcessUnexpectedErrorAsync(Exception ex);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _bot.StartReceiving(ProcessUpdate, ProcessError, _receiverOptions, _cancellationToken.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationToken.Cancel();
        }

        public async Task ProcessUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                IncomingMessage message = await ProcessIncomingMessage(update);
                if (String.IsNullOrWhiteSpace(message.CommandName)) return;
                Type? type = Type.GetType(message.CommandName);
                if (type is null) return;
                if (!type.IsSubclassOf(typeof(BotCommand))) return;
                object? command = Activator.CreateInstance(type);
                if (command is null) return;
                (command as BotCommand).Message = message;
                object? a = await _mediator.Send(command);
                if (a is null) return;
                if (a is List<Answer>)
                {
                    var answers = (List<Answer>)a;
                    foreach (var answer in answers)
                    {
                        PrepareAnswer(answer);

                        if (answer.AnswerType == Answer.AnswerTypes.Text)
                        {
                            await _bot.SendMessage(chatId: update.Message.Chat.Id,
                                linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = answer.DisableWebPagePreview },
                                parseMode: answer.IsHtml ? ParseMode.Html : ParseMode.MarkdownV2,
                                text: answer.Text.ToString(CultureInfo.CreateSpecificCulture("en-GB")),
                                replyParameters: answer.ReplyToMessageId.HasValue ? new ReplyParameters() { MessageId = answer.ReplyToMessageId.Value } : null,
                                cancellationToken: new CancellationTokenSource(1000).Token
                                );
                        }
                        if (answer.AnswerType == Answer.AnswerTypes.Reaction)
                        {
                            await _bot.SetMessageReaction(chatId: update.Message.Chat.Id,
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
                            await _bot.SendPhoto(chatId: update.Message.Chat.Id, new InputFileStream(ms, answer.Text));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ProcessUnexpectedErrorAsync(ex);
            }
        }

        public async Task<IncomingMessage> ProcessIncomingMessage(Update update)
        {
            IncomingMessage incomingMessage = new IncomingMessage()
            {
                UpdateType = update.Type.ToString().ToLower()
            };

            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        var message = update.Message;
                        incomingMessage.Text = message.Text;
                        incomingMessage.ChatId = message.Chat.Id;
                        incomingMessage.MessageId = message.MessageId;
                        break;
                }

                incomingMessage = PrepareIncommingMessageCommandName(incomingMessage);
            }
            catch(Exception ex)
            {
                await ProcessUnexpectedErrorAsync(ex);
            }

            return incomingMessage;
        }

        public async Task ProcessError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
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
    }
}
