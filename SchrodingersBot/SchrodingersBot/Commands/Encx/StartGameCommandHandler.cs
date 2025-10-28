using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class StartGameCommandHandler : IBotCommandHandler<startgameCommand>
    {
        public Task<List<Answer>> Handle(startgameCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}
