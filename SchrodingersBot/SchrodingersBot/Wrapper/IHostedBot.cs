using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public interface IHostedBot<T> : IHostedService
        where T : BotOptions, new()
    {

    }
}
