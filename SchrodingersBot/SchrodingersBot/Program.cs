using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotABot.Wrapper;
using SchrodingersBot.Services.Location;
using SchrodingersBot.Services.Text;

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

                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                Configure(services, context.Configuration);
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

            //Main bot
            services.AddHostedService<GolfBot>();

            //Daemons
            //services.AddSingleton<TodoOneMinuteDaemon>();
            //services.AddHostedService<BackgroundServiceStarter<TodoOneMinuteDaemon>>();

            //MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            ////services.AddScoped<MediatR.IMediator, MediatR.Mediator>();
            ////services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotBotCommandHandler).Assembly));
            
            //Services
            services.AddScoped(typeof(ICoordinatesProcessingService), typeof(CoordinatesProcessingService));
            services.AddScoped(typeof(ITextProcessingService), typeof(TextProcessingService));
        }

    }
}
