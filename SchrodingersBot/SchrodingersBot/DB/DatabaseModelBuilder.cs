using Microsoft.EntityFrameworkCore;
using SchrodingersBot.DB.DBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB
{
    public static class DatabaseModelBuilder
    {
        public static void ModelCreating(ModelBuilder builder)
        {
            builder.Entity<EncxGameSubscriptionEntity>(entity =>
            {
                
            });

            builder.Entity<EncxAuthEntity>(entity =>
            {
                entity.HasMany(auth => auth.Subscriptions)
                .WithOne(sub => sub.LoginInfo)
                .HasForeignKey(sub => sub.LoginInfoId);
            });
        }
    }
}
