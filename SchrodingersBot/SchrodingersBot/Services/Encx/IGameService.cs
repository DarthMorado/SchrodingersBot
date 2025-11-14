using SchrodingersBot.DB.DBO;
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
    }
}
