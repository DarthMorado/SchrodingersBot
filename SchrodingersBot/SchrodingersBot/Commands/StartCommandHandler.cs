using Microsoft.Extensions.Options;
using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class StartCommandHandler : IBotCommandHandler<startCommand>
    {
        private readonly GolfBotOptions _options;

        public StartCommandHandler(IOptions<GolfBotOptions> options
            )
        {
            _options = options.Value;
        }

        public async Task<List<Answer>> Handle(startCommand request, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("This is Bot");
            
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            sb.AppendLine($"v.{version}");

            
            if (!String.IsNullOrEmpty(_options.RunInfo))
            {
                sb.AppendLine($"Run info: {_options.RunInfo}");
            }

            return new()
            {
                new()
                {
                    ChatId = request.Message.ChatId,
                    AnswerType = "message",
                    Text = sb.ToString()
                }
            };
        }
    }
}
