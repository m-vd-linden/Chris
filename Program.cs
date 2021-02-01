using Chris.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Chris
{
    class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("_config.yml");
            Configuration = builder.Build();

            var provider = ConfigureServices();

            var client = provider.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await provider.GetRequiredService<StartupService>().StartAsync();
            await provider.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,
                    GatewayIntents =
                            GatewayIntents.Guilds |
                            GatewayIntents.GuildPresences |
                            GatewayIntents.GuildMembers
                }))
                .AddSingleton<CommandService>()
                .AddSingleton<StartupService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton(Configuration)
                .BuildServiceProvider();
        }
    }
}
