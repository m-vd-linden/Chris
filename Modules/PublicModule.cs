using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chris.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("pong", "hello")]
        [RequireOwner]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("userinfo")]
        [RequireOwner]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("echo")]
        [RequireOwner]
        public Task EchoAsync([Remainder] string text)
            => ReplyAsync('\u200B' + text);
    }
}
