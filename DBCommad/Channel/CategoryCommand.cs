using DiscordBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBotCore.DBCommad.Channel
{
    [Group("category"), Aliases("group")]
    public class CategoryCommand
    {
        [GroupCommand()]
        public async Task CategoryCommandAsync(CommandContext ctx) {
            DiscordGuild guild = ctx.Guild;
            List<string> list_category_name = new List<string>();
            foreach (KeyValuePair<ulong, DiscordChannel> item in guild.Channels)
            {
                if (item.Value.IsCategory || item.Value.Type == ChannelType.Category)
                {
                    list_category_name.Add(item.Value.Name);
                }
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = guild.Name,
                    IconUrl = guild.IconUrl
                },
            }.AddField("Total category: "+list_category_name.Count, string.Join(',', list_category_name)));
        }

        [Command("new")]
        public async Task NewCommandAsync(CommandContext ctx, 
            [Description("Name of category, didnt restrict like channel cmd"), RemainingText]string name) {
            try
            {
                DiscordChannel new_ = await ctx.Guild.CreateChannelCategoryAsync(name, reason:"Create by "+ctx.Member.Username+" with QuickBot");
                await ctx.RespondAsync("Create category succeed! ID: "+new_.Id.ToString());
            }
            catch (Exception e)
            {
                if (e is DSharpPlus.Exceptions.BadRequestException || e is DSharpPlus.Exceptions.ServerErrorException)
                {
                    await ctx.Channel.SendMessageAsync("Bad request or server error exception, please try again");
                }
                else if (e is DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.Channel.SendMessageAsync("Not found, please run this command in server, not DM");
                }
                else if (e is DSharpPlus.Exceptions.UnauthorizedException)
                {
                    await ctx.Channel.SendMessageAsync("Unauthoried, please check your permission to run this command");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder { Description = "```"+e.Message+"```" });
                }
            }
        }
    }
}
