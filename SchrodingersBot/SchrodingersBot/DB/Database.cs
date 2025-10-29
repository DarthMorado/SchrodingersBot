using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchrodingersBot.DB.DBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options)
            :base(options)
        {
            
        }

        public DbSet<AreaEntity> Areas { get; set; }
        public DbSet<EncxLoginInfoEntity> LoginInfos { get; set; }
        
    }
}
