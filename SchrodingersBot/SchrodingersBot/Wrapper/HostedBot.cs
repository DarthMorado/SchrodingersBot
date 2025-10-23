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
            IncomingMessage message = ProcessIncomingMessage(update);
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
                    await _bot.SendMessage(chatId: update.Message.Chat.Id,
                        linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = answer.DisableWebPagePreview },
                        parseMode: answer.IsHtml ? ParseMode.Html : ParseMode.MarkdownV2,
                        text: answer.Text.ToString(CultureInfo.CreateSpecificCulture("en-GB")),
                        replyParameters: answer.ReplyToMessageId.HasValue ? new ReplyParameters() { MessageId = answer.ReplyToMessageId.Value } : null,
                        cancellationToken: new CancellationTokenSource(1000).Token
                        );
                }
            }
        }

        public IncomingMessage ProcessIncomingMessage(Update update)
        {
            IncomingMessage incomingMessage = new IncomingMessage()
            {
                UpdateType = update.Type.ToString().ToLower()
            };

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
            return incomingMessage;
        }

        public async Task ProcessError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
        }
    }
}
