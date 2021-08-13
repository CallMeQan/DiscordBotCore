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
    [Group("voice"), Aliases("vc")]
    public class VoiceCommand : BaseCommandModule
    {
        #region Base Command
        [Command("disconnect"), RequirePermissions(Permissions.MoveMembers)]
        public async Task DisconnectMemberAsync(CommandContext ctx, [Description("")] DiscordMember member)
        {
            if (member.VoiceState.Channel != null)
            {
                await member.ModifyAsync(async x =>
                {
                    x.VoiceChannel = null;
                    await ctx.RespondAsync("Succeed disconnect user from voice :)");
                });
            }
            else
                await ctx.RespondAsync("Can't disconnect user, user are not in any voice channel");
        }

        [Command("move"), RequirePermissions(Permissions.MoveMembers)]
        public async Task MoveMemberAsync(CommandContext ctx, [Description("")] DiscordMember member, [Description("")] ulong channelId)
        {
            if (member.VoiceState.Channel.Id == channelId)
                await ctx.RespondAsync("User already in that voice");
            else
            {
                await member.ModifyAsync(x => x.VoiceChannel = ctx.Guild.GetChannel(channelId));
                await ctx.RespondAsync("Succeed move user ");
            }
        }

        [Command("mute"), RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteMemberAsync(CommandContext ctx, [Description("")] DiscordMember member)
        {
            if (member.VoiceState.Channel == null)
                await ctx.RespondAsync("User aren't in any voice channel");
            else
            {
                await member.ModifyAsync(x => x.Muted = true);
                await ctx.RespondAsync("Muted " + member.Mention);
            }
        }

        [Command("deafen"), RequirePermissions(Permissions.DeafenMembers), Aliases("deaf")]
        public async Task DeafenMember(CommandContext ctx, [Description("")] DiscordMember member)
        {
            if (member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("User aren't in any voice channel");
            }
            else
            {
                await member.ModifyAsync(x => x.Deafened = true);
                await ctx.RespondAsync("Deafened " + member.Mention);
            }
        }
        #endregion
        private const string RemainingTextNewCmdDescription = "name[separate by quote] -> bitrates[optional] -> user_limit[optional]";
        private const string NewCmdDescription = "";
        [Command("new"), Aliases("create"), RequirePermissions(Permissions.ManageChannels)]
        public async Task NewVoiceChannelAsync(CommandContext ctx, 
            [Description("")] DiscordChannel category, 
            [Description(RemainingTextNewCmdDescription),RemainingText] string txt)
        {
            DiscordGuild guild = ctx.Guild;

            if (guild != null)
            {
                ResultConverterNewCmd resultConverter = VoiceCommandConverter.NewVoiceChannelCmdConverter(txt);

                DiscordChannel result = await guild.CreateVoiceChannelAsync(
                    resultConverter.name, category, resultConverter.bitrates, resultConverter.user_limit, qualityMode: VideoQualityMode.Auto,
                    reason: "Request create new voice channel by " + ctx.Member.Username + " , from QuickServer bot"
                );
            }
        }

        [Command("new"), Aliases("create"), RequirePermissions(Permissions.ManageChannels)]
        public async Task NewVoiceChannelAsync(CommandContext ctx, 
            [Description(RemainingTextNewCmdDescription),RemainingText] string txt)
        {
            DiscordGuild guild = ctx.Guild;
            if (guild != null)
            {
                ResultConverterNewCmd resultConverter = VoiceCommandConverter.NewVoiceChannelCmdConverter(txt);

                DiscordChannel result = await guild.CreateVoiceChannelAsync(
                    resultConverter.name, bitrate: resultConverter.bitrates, user_limit: resultConverter.user_limit, qualityMode: VideoQualityMode.Auto,
                    reason: "Request create new voice channel by " + ctx.Member.Username + " , from QuickServer bot"
                );
            }
        }
    }
}
