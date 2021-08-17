using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        public readonly EventId BotEventId = new EventId(42, "QuickServer");

        public DiscordClient client { get; set; }
        public CommandsNextExtension commands { get; set; }
        private static readonly string TOKEN = "ODU3ODY5MTQ1MDUyMDIwNzM2.YNV25g.hzf3lniPYjXGOhg-ODXf2fg8ZtY";

        static void Main(string[] args)
        {
            new Program().RunMainAsync().GetAwaiter().GetResult();
        }

        async Task RunMainAsync()
        {
            DiscordConfiguration configuration = new DiscordConfiguration
            {
                Token = TOKEN,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };
            CommandsNextConfiguration commandConfig = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] { "sv." },

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true,
                EnableDms = false
            };

            this.client = new DiscordClient(configuration);

            this.client.Ready += this.OnReady;
            this.client.GuildAvailable += this.GuildAvailable;
            this.client.ClientErrored += this.Client_ClientError;
            this.commands = this.client.UseCommandsNext(commandConfig);

            this.commands.RegisterCommands<DiscordBot.DBCommand.Channel.ChannelCommand>();
            this.commands.RegisterCommands<DiscordBot.DBCommand.ServerCommand>();
            this.commands.RegisterCommands<DiscordBot.DBCommand.Channel.VoiceCommand>();
            this.commands.RegisterCommands<DiscordBot.DBCommad.ModerationCommand>();

            this.commands.CommandExecuted += this.Commands_CommandExecuted;
            this.commands.CommandErrored += this.Commands_CommandErrored;

            await this.client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
            return Task.CompletedTask;
        }

        private Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }
        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed);
            }
        }
    }
}
