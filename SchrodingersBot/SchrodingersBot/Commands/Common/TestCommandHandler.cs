using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class TestCommandHandler : IBotCommandHandler<testCommand>
    {
        public async Task<List<Answer>> Handle(testCommand request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
