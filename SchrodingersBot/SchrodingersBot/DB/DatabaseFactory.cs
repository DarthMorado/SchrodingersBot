using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB
{
    public class DatabaseFactory : IDesignTimeDbContextFactory<Database>
    {
        public Database CreateDbContext(string[] args)
        {
            // 1. Build the Configuration manually for the design-time process
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.Setup.json", optional: true, reloadOnChange: true)
                .Build();

            // 2. Get the Connection String
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 3. Configure the DbContextOptions
            var builder = new DbContextOptionsBuilder<Database>();
            builder.UseSqlServer(connectionString);

            // 4. Return a new instance of your DbContext
            return new Database(builder.Options);
        }
    }
}
