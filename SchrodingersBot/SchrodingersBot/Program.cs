using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotABot.Wrapper;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.Services.Location;
using SchrodingersBot.Services.Text;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Logging;
using SchrodingersBot.Services.Logging;
using SchrodingersBot.Services.Encx;

namespace SchrodingersBot
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.AddJsonFile($"appsettings.Setup.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                Configure(services, context.Configuration);
            })
            .ConfigureLogging(log =>
            {
                log.ClearProviders();
                log.AddConsole();

                log.AddEventLog(new EventLogSettings
                {
                    SourceName = "Morado.TG",
                    LogName = "SchrodingersBot"
                });

                log.AddProvider(new BotLoggerProvider());
            });



#if DEBUG
            // Run as a console app in Debug mode
            builder.UseConsoleLifetime();
#else
        // Run as a Windows Service in Release mode
        builder.UseWindowsService()
               .UseContentRoot(AppContext.BaseDirectory);
#endif

            var host = builder.Build();
            host.Run();
        }

        public static void Configure(IServiceCollection services, IConfiguration config)
        {
            // Options
            services.Configure<GolfBotOptions>(config.GetSection("BotOptions"));
            services.AddAutoMapper(x => x.AddProfile(typeof(AutoMapperProfile)));

            //MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            ////services.AddScoped<MediatR.IMediator, MediatR.Mediator>();
            ////services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotBotCommandHandler).Assembly));

            // Database
            ConfigureDatabase(services, config);

            //Main bot
            services.AddHostedService<GolfBot>();

            //Daemons
            //services.AddSingleton<TodoOneMinuteDaemon>();
            //services.AddHostedService<BackgroundServiceStarter<TodoOneMinuteDaemon>>();

            
            
            //Services
            services.AddScoped(typeof(ICoordinatesProcessingService), typeof(CoordinatesProcessingService));
            services.AddScoped(typeof(ITextProcessingService), typeof(TextProcessingService));
            services.AddScoped(typeof(IAreasService), typeof(AreasService));
            services.AddScoped<IEncxService, EncxService>();
        }

        public static void ConfigureDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<Database>(options =>
                options.UseSqlServer(connectionString));
            //services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped(typeof(IDbRepository<>), typeof(DbRepository<>));
            

        }
    }
}
