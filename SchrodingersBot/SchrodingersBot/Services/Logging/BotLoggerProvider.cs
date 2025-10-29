using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Logging
{
    public class BotLoggerProvider : ILoggerProvider
    {
        

        public BotLoggerProvider()
        {
            
        }

        public ILogger CreateLogger(string categoryName) =>
            new BotLogger();

        public void Dispose() { }
    }
}
