using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DTO.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Encx
{
    public interface IGameService
    {
        public Task<EncxGameSubscriptionEntity?> GetActiveGame(long chatId);
        public Task<Result> FormatGameState(IncomingMessage message, EncxGameEngineModel game, bool needScreenshot = false);
    }
}
