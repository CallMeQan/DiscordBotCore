using DiscordBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.DBCommand.Channel
{
    [Group("channel")]
    [RequirePermissions(Permissions.ManageChannels)] // and restrict this to users who have appropriate permissions
    public class ChannelCommand : BaseCommandModule
    {
        [GroupCommand()]
        public async Task ChannelCommandAsync(CommandContext ctx) {
            DiscordGuild guild = ctx.Guild;
            List<string> list_text_channel = new List<string>();
            foreach (KeyValuePair<ulong, DiscordChannel> item in guild.Channels)
            {
                if (item.Value.Type == ChannelType.Text || item.Value.Type == ChannelType.News || item.Value.Type == ChannelType.Private)
                {
                    list_text_channel.Add(item.Value.Name);
                }
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = guild.Name,
                    IconUrl = guild.IconUrl
                },
            }.AddField("Total text channel: " + list_text_channel.Count, string.Join(',', list_text_channel)));
        }

        #region checkperms command
        [Command("checkperms")]
        public async Task CheckPermsAsync(CommandContext ctx,
            [Description("Channel mentioned")] DiscordChannel channel)
        {
            PermsList permsList = new PermsList();
            foreach (DiscordOverwrite overwrite in channel.PermissionOverwrites)
            {
                DiscordGuild guild = channel.Guild;
                if (overwrite.Type == OverwriteType.Member)
                {
                    permsList.users.Add(new UserOverwrite(guild.GetMemberAsync(overwrite.Id).Result, overwrite.Allowed, overwrite.Denied));
                }
                else if (overwrite.Type == OverwriteType.Role)
                {
                    permsList.roles.Add(new RoleOverwrite(guild.GetRole(overwrite.Id), overwrite.Allowed, overwrite.Denied));
                }
            };

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            foreach (RoleOverwrite item in permsList.roles)
            {
                embed.AddField(item.role.Name, string.Concat(new string[] {
                    "Role: <@&"+item.role.Id+">\n",
                    "Allowed: "+item.Allowed.ToString(),
                    "\n",
                    "Denied: "+item.Denied.ToString()
                }));
            }

            foreach (UserOverwrite item in permsList.users)
            {
                embed.AddField(item.member.Username, string.Concat(new string[] {
                    "User: <@"+item.member.Id+">\n",
                    "Allowed: "+item.Allowed.ToString(),
                    "\n",
                    "Denied: "+item.Denied.ToString()
                }));
            }
            embed.Title = "#" + channel.Name;
            await ctx.RespondAsync(embed);
        }
        #endregion

        #region addoverwrite command
        private const string DescriptionAddPermsArgs = "Some permission, PERMISSION ARE SPLIT AS `,`";
        private const string DescriptionAddPermChannel = "Channel need to overwrited";
        private const string DescriptionAddPermUserOrRole = "User or role need to overwrite in that channel, add new if doesnt exist";
        [Command("addoverwrite"), Description("Add overwrite for role")]
        public async Task AddOverwriteAsync(CommandContext ctx,
            [Description(DescriptionAddPermChannel)] DiscordChannel channel,
            [Description(DescriptionAddPermUserOrRole)] DiscordRole role,
            [Description(DescriptionAddPermsArgs), RemainingText] string perms_fixed)
        => await AddOverwriteChannel(ctx, channel, role, perms_fixed);

        [Command("addoverwrite"),Description("Add overwrite for member")]
        public async Task AddOverwriteAsync(CommandContext ctx,
            [Description(DescriptionAddPermChannel)] DiscordChannel channel,
            [Description(DescriptionAddPermUserOrRole)] DiscordMember member,
            [Description(DescriptionAddPermsArgs), RemainingText] string perms_fixed)
            => await AddOverwriteChannel(ctx, channel, member, perms_fixed);

        private async Task AddOverwriteChannel(CommandContext ctx, DiscordChannel channel, object UserOrRole, string perms_need_to_add)
        {
            Permissions result = PermissionChecker.ConvertPerms(perms_need_to_add);
            DiscordEmbedBuilder builder_respond = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Member.Username + " added new overwrite to " + channel.Name,
                    IconUrl = ctx.Member.AvatarUrl
                }
            };
            try
            {
                if (UserOrRole.GetType() == typeof(DiscordMember))
                {
                    DiscordMember mem = (DiscordMember)UserOrRole;
                    await channel.AddOverwriteAsync(mem, allow: result);
                    builder_respond.AddField(mem.Username + " with id " + mem.Id + " added", result.ToPermissionString(), true);
                }
                else if (UserOrRole.GetType() == typeof(DiscordRole))
                {
                    DiscordRole role = (DiscordRole)UserOrRole;
                    await channel.AddOverwriteAsync(role, allow: result);
                    builder_respond.AddField(role.Name + " with id " + role.Id + " added", result.ToPermissionString(), true);
                }
                else
                    builder_respond.Description = "Argument error: type of `user` or `role` is invalid";
                await ctx.RespondAsync(builder_respond);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
            }
        }
        #endregion

        #region removeoverwrite
        [Command("removeoverwrite"), Aliases("removeuser"), Description("Remove overwrite of member")]
        public async Task RemoveOverwriteAsync(CommandContext ctx,
            [Description(DescriptionAddPermChannel)] DiscordChannel channel,
            [Description(DescriptionAddPermUserOrRole)] DiscordMember member,
            [Description(DescriptionAddPermsArgs), RemainingText] string perms_fixed)
        => await RemoveOverwriteChannel(ctx, channel, member, perms_fixed);

        [Command("removeoverwrite"), Aliases("removerole"), Description("Remove overwrite of role")]
        public async Task RemoveOverwriteAsync(CommandContext ctx,
            [Description(DescriptionAddPermChannel)] DiscordChannel channel,
            [Description(DescriptionAddPermUserOrRole)] DiscordRole role,
            [Description(DescriptionAddPermsArgs), RemainingText] string perms_fixed)
        => await RemoveOverwriteChannel(ctx, channel, role, perms_fixed);

        private async Task RemoveOverwriteChannel(CommandContext ctx, DiscordChannel channel, object UserOrRole, string perms_need_to_remove)
        {
            Permissions result = PermissionChecker.ConvertPerms(perms_need_to_remove);
            DiscordEmbedBuilder builder_respond = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Member.Username + " removed overwrite for " + channel.Name,
                    IconUrl = ctx.Member.AvatarUrl
                }
            };
            try
            {
                if (UserOrRole.GetType() == typeof(DiscordMember))
                {
                    DiscordMember mem = (DiscordMember)UserOrRole;
                    await channel.AddOverwriteAsync(mem, deny: result);
                    builder_respond.AddField(mem.Username + " with id " + mem.Id + " removed", result.ToPermissionString(), true);
                }
                else if (UserOrRole.GetType() == typeof(DiscordRole))
                {
                    DiscordRole role = (DiscordRole)UserOrRole;
                    await channel.AddOverwriteAsync(role, deny: result);
                    builder_respond.AddField(role.Name + " with id " + role.Id + " removed", result.ToPermissionString(), true);
                }
                else
                    builder_respond.Description = "Argument error: type of `user` or `role` is invalid";
                builder_respond.Color = DiscordColor.Magenta;
                await ctx.RespondAsync(builder_respond);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
            }
        }
        #endregion

        #region allow cmd
        [Command("allow")]
        public async Task AllowCommandAsync(CommandContext ctx,
            [Description("Who? ping or id")] DiscordMember member,
            [Description("Which? mention or id")] DiscordChannel channel) 
        {
            bool yesss = false;
            Permissions allow = Permissions.None;
            Permissions deny = Permissions.None;
            foreach (DiscordOverwrite overwrite in channel.PermissionOverwrites)
            {
                if (overwrite.Type == OverwriteType.Member)
                {
                    DiscordMember mem_overwrite = await overwrite.GetMemberAsync();
                    if (mem_overwrite.Id == member.Id)
                    {
                        allow = overwrite.Allowed;
                        deny = overwrite.Denied;
                        yesss = true;
                    }
                }
            }
            List<Permissions> perms_to_allow = new List<Permissions> { Permissions.AccessChannels, Permissions.ReadMessageHistory, Permissions.SendMessages };
            if (yesss)
            {
                allow = PermissionChecker.GrantByList(allow, perms_to_allow);
                await channel.AddOverwriteAsync(member, allow: allow, deny: deny);
                await ctx.RespondAsync("Allowed " + member.Mention+" to "+channel.Mention);
            }
            else
            {
                await channel.AddOverwriteAsync(member, allow: PermissionChecker.GrantByList(allow, perms_to_allow));
                await ctx.RespondAsync("Allowed " + member.Mention +" to "+channel.Mention);
            }
        }
        #endregion

        #region new cmd
        [Command("new")]
        public async Task NewCommandAsync(CommandContext ctx,
            [Description("Name of the new channel, DO NOT ADD WHITESPACE")] string name,
            [Description("Time of slowmode, 0 = disable slow mode, max 21600s (6h), default disable")] int slowmodesecs = 0,
            [Description("nsfw enable? (Not safe for work)")] string isNSFW = "no")
        {
            try
            {
                ChannelCommandConverter.ResultConverterNewCmd resultConverter = ChannelCommandConverter.NewCommandConverter(name, slowmodesecs, isNSFW);
                await ctx.Guild.CreateChannelAsync(resultConverter.name, ChannelType.Text, 
                    nsfw:resultConverter.isNsfw, perUserRateLimit:resultConverter.slowtime);
                await ctx.RespondAsync("Create channel succeed!");
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
            }
        }
        [Command("new")]
        public async Task NewCommandAsync(CommandContext ctx, 
            [Description("Category mention or id")] DiscordChannel category, 
            [Description("Name of the new channel, DO NOT ADD WHITESPACE")] string name,
            [Description("Time of slowmode, 0 = disable slow mode, max 21600s (6h), default disable")] int slowmodesecs = 0,
            [Description("nsfw enable? (Not safe for work)")] string isNSFW = "no")
        {
            if (!category.IsCategory)
            {
                await ctx.RespondAsync("Argument category is invalid!");
                return;
            }
            try
            {
                ChannelCommandConverter.ResultConverterNewCmd resultConverter = ChannelCommandConverter.NewCommandConverter(name, slowmodesecs, isNSFW);
                await ctx.Guild.CreateChannelAsync(resultConverter.name, ChannelType.Text, 
                    nsfw:resultConverter.isNsfw, perUserRateLimit:resultConverter.slowtime, 
                    parent:category);
                await ctx.RespondAsync("Create channel succeed!");
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
            }   
        }

        #endregion
    }

    #region no need to care
    internal class PermsList
    {
        public List<RoleOverwrite> roles = new List<RoleOverwrite>();
        public List<UserOverwrite> users = new List<UserOverwrite>();
        public RoleOverwrite everyone { get; set; }
    }

    class RoleOverwrite
    {
        public RoleOverwrite(DiscordRole result, Permissions allowed, Permissions denied)
        {
            Allowed = allowed;
            Denied = denied;
            role = result;
        }

        public DiscordRole role { get; set; }
        public Permissions Allowed { get; set; }
        public Permissions Denied { get; set; }
    }

    class UserOverwrite
    {
        public UserOverwrite(DiscordMember result, Permissions allowed, Permissions denied)
        {
            member = result;
            Allowed = allowed;
            Denied = denied;
        }

        public DiscordMember member { get; set; }
        public Permissions Allowed { get; set; }
        public Permissions Denied { get; set; }
    }
    #endregion
}
