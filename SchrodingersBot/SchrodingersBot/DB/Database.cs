using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchrodingersBot.DB;
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
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DatabaseModelBuilder.ModelCreating(modelBuilder);
        }

        public DbSet<AreaEntity> Areas { get; set; }
        public DbSet<EncxAuthEntity> LoginInfos { get; set; }
        public DbSet<EncxGameSubscriptionEntity> GameSubscriptions {get;set;}
        public DbSet<EncxLevelEntity> Levels { get; set; }
        public DbSet<EncxObjectEntity> Objects { get; set; }

    }
}
