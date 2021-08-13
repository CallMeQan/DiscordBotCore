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
        [Command("addoverwrite"), Aliases("addrole"), Description("Add overwrite for role")]
        public async Task AddOverwriteAsync(CommandContext ctx,
            [Description(DescriptionAddPermChannel)] DiscordChannel channel,
            [Description(DescriptionAddPermUserOrRole)] DiscordRole role,
            [Description(DescriptionAddPermsArgs), RemainingText] string perms_fixed)
        => await AddOverwriteChannel(ctx, channel, role, perms_fixed);

        [Command("addoverwrite"), Aliases("adduser"), Description("Add overwrite for member")]
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
