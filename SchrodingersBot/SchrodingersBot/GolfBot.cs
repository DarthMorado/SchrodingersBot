using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot
{
    public class GolfBot : HostedBot<GolfBotOptions>
    {
        private readonly ILogger<GolfBot> _logger;

        private string BotCommandNamespace = "SchrodingersBot.Commands.";

        public GolfBot(IMediator mediator, IOptions<GolfBotOptions> options, ILogger<GolfBot> logger)
            : base(mediator, options.Value)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
        }

        public override IncomingMessage PrepareIncommingMessageCommandName(IncomingMessage incomingMessage)
        {
            if (incomingMessage == null)
            {
                return incomingMessage;
            }    

            switch (incomingMessage.UpdateType)
            {
                case "message":
                    string text = incomingMessage.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        break;
                    }
                    if (text.StartsWith("."))
                    {
                        incomingMessage.CommandName = $"DotCommand";
                    }
                    else if (!String.IsNullOrEmpty(incomingMessage.Command))
                    {
                        incomingMessage.CommandName = $"{incomingMessage.Command}Command";
                    }
                    break;
            }
            if (String.IsNullOrEmpty(incomingMessage.CommandName))
            {
                incomingMessage.CommandName = "emptyCommand";
            }

            incomingMessage.CommandName = $"{BotCommandNamespace}{incomingMessage.CommandName}";
            return incomingMessage;
        }

        public override async Task ProcessUnexpectedErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error!");
        }
    }
}
