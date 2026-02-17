using Microsoft.EntityFrameworkCore;
using SchrodingersBot.DB.DBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.Repositories
{
    public interface IDbChatRepository<T> : IDbRepository<T>
        where T : ChatEntity
    {
        public Task<List<T>> GetByChatIdAsync(long chatId);
    }
}
