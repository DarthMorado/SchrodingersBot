using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class StartCommandHandler : IBotCommandHandler<startCommand>
    {
        public async Task<List<Answer>> Handle(startCommand request, CancellationToken cancellationToken)
        {
            return new()
            {
                new()
                {
                    ChatId = request.Message.ChatId,
                    AnswerType = "message",
                    Text =
@"This is Bot
v\.0\.1\.
"
                }
            };
        }
    }
}
