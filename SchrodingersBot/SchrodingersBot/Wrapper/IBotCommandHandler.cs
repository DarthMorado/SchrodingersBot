using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public interface IBotCommandHandler<T> : IRequestHandler<T, List<Answer>>
        where T : BotCommand
    {
        
    }
}
