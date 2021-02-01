using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Chris.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;

        public CommandHandlingService(IServiceProvider services,
            IConfigurationRoot config)
        {
            _config = config;
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += MessageReceivedAsync;

            JobManager.Initialize();
            JobManager.AddJob(
                ChooseBeroerdste,
                s => s.ToRunEvery(0).Weeks().On(DayOfWeek.Wednesday).At(19, 00));
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }


        private async void ChooseBeroerdste()
        {
            Random rnd = new Random();

            var dankMemesId = Convert.ToUInt64(_config["servers:DankMemes"]);
            SocketGuild dankGuild = _discord.Guilds.Where(x => x.Id == dankMemesId).Single();

            var beroerdeRole = dankGuild.Roles.Where(x => x.Name == "De Beroerdste").First();
            var innerCircle = dankGuild.Roles.Where(x => x.Name == "Inner Circle").First();

            var beroerdeUsers = dankGuild.Users.Where(x => x.Roles.Contains(beroerdeRole)).ToList();
            var unberoerdeUsers = dankGuild.Users.Except(beroerdeUsers).Where(x => x.Roles.Contains(innerCircle));

            var newBeroerdste = unberoerdeUsers.ElementAt(rnd.Next(unberoerdeUsers.Count()));
            foreach (SocketGuildUser user in beroerdeUsers)
            {
                await user.RemoveRoleAsync(beroerdeRole);
            }

            await newBeroerdste.AddRoleAsync(beroerdeRole);
            await dankGuild.TextChannels.Where(x => x.Name == "general").First().SendMessageAsync($"Gefeliciteerd {newBeroerdste.Mention}, jij bent écht beroerd zeg");
        }
    }
}
