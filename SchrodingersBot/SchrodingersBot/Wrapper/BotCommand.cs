using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public class BotCommand : IRequest<List<Answer>>
    {
        public IncomingMessage Message { get; set; }
    }
}
