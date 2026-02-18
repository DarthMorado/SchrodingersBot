using Microsoft.EntityFrameworkCore;
using SchrodingersBot.DB.DBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.Repositories
{
    public class DbChatRepository<T> : DbRepository<T>, IDbChatRepository<T>
        where T : ChatEntity
    {
        public DbChatRepository(Database context)
            :base(context)
        {
            
        }

        public async Task<List<T>> GetByChatIdAsync(long chatId)
        {
            var result = await _dbSet.Where(x => x.ChatId == chatId).ToListAsync();
            return result ?? new List<T>();
        }

    }
}
