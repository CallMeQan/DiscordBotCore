using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DiscordBot.DBCommand
{
    [Group("server"), RequirePermissions(Permissions.ManageGuild), Aliases("sv")]
    public class ServerCommand : BaseCommandModule
    {
        [Command("serverinfo")]
        public async Task ShowInfoAsync(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            DiscordGuild guild = ctx.Guild;

            embed.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = guild.IconUrl,
                Text = guild.Id.ToString()
            };
            embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = guild.IconUrl,
                Width = 150,
                Height = 150
            };
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = "Name: " + guild.Name,
                IconUrl = guild.IconUrl
            };
            int categoryCount = 0;
            int unknowChannelCount = 0;
            int voiceCount = 0;
            int textChannelCount = 0;
            foreach (ulong channelID in guild.Channels.Keys)
            {
                switch (guild.GetChannel(channelID).Type)
                {
                    case ChannelType.Text:
                        textChannelCount++;
                        break;
                    case ChannelType.Voice:
                        voiceCount++;
                        break;
                    case ChannelType.Category:
                        categoryCount++;
                        break;
                    case ChannelType.Stage:
                        voiceCount++;
                        break;
                    case ChannelType.Unknown:
                        unknowChannelCount++;
                        break;
                    default:
                        break;
                }
            }

            embed.AddField("Total categorys", categoryCount.ToString(), true)
                .AddField("Total text channels", textChannelCount.ToString(), true)
                .AddField("Total voice channels", voiceCount.ToString(), true)
                .AddField("Total UNKNOW channels", unknowChannelCount.ToString(), true)
                .AddField("Total CHANNEL", (textChannelCount + voiceCount + unknowChannelCount).ToString(), true)
                .AddField("Total members", guild.MemberCount.ToString(), true);

            embed.Color = DiscordColor.Magenta;
            await ctx.RespondAsync(embed);
        }

        [Command("totalmember")]
        public async Task TotalMemberAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(ctx.Guild.MemberCount.ToString());
        }

        [Command("totalchannel")]
        public async Task TotalChannelAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(ctx.Guild.Channels.Count.ToString());
        }

        [Command("vcbot"), Description("")]
        public async Task VcBotCommandAsync(CommandContext ctx) {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder { 
                Author = new DiscordEmbedBuilder.EmbedAuthor { 
                    Name=ctx.Member.Username,
                    IconUrl = ctx.Member.AvatarUrl
                },
                Description = "Currently bot in voice (in used):\n",
                Color = DiscordColor.Azure
            };

            foreach (KeyValuePair<ulong, DiscordChannel> item in ctx.Guild.Channels)
            {
                if (item.Value.Type == ChannelType.Voice)
                {
                    foreach (DiscordMember mem in item.Value.Users)
                    {
                        if (mem.IsBot) {
                            embedBuilder.Description = embedBuilder.Description + mem.Mention + " ";
                        }
                    }
                }
            }
            await ctx.RespondAsync(embedBuilder);
        }
    }
}
