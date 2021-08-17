using DiscordBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.DBCommand.Channel
{
    [Group("voice"), Aliases("vc")]
    public class VoiceCommand : BaseCommandModule
    {
        [GroupCommand()]
        public async Task VoiceCommandAsync(CommandContext ctx) {
            DiscordGuild guild = ctx.Guild;
            List<string> list_voice_name = new List<string>();
            foreach (KeyValuePair<ulong, DiscordChannel> item in guild.Channels)
            {
                if (item.Value.Type == ChannelType.Voice || item.Value.Type == ChannelType.Stage)
                {
                    list_voice_name.Add(item.Value.Name);
                }
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = guild.Name,
                    IconUrl = guild.IconUrl
                },
            }.AddField("Total voice: "+list_voice_name.Count, string.Join(',', list_voice_name)));
        }

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

        [Command("mute"), RequirePermissions(Permissions.MuteMembers), Description("")]
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

        #region new cmd
        private const string RemainingTextNewCmdDescription = "name[separate by quote] -> bitrates[optional] -> user_limit[optional]";
        private const string NewCmdDescription = "";
        [Command("new"), RequirePermissions(Permissions.ManageChannels), Description(NewCmdDescription)]
        public async Task NewVoiceChannelAsync(CommandContext ctx, 
            [Description("")] DiscordChannel category, 
            [Description(RemainingTextNewCmdDescription),RemainingText] string txt)
        {
            DiscordGuild guild = ctx.Guild;

            if (guild != null)
            {
                try
                {
                    VoiceCommandConverter.ResultConverterNewCmd resultConverter = VoiceCommandConverter.NewVoiceChannelCmdConverter(txt);

                    DiscordChannel result = await guild.CreateVoiceChannelAsync(
                        resultConverter.name,parent: category, bitrate: resultConverter.bitrates, user_limit: resultConverter.user_limit, qualityMode: VideoQualityMode.Auto,
                        reason: "Request create new voice channel by " + ctx.Member.Username + " , from QuickServer bot"
                    );
                }catch(Bad​Request​Exception e)
                {
                    string error_mes = string.Concat(new string[] {
                        "Response: ```json\n",
                        e.WebResponse.Response+"\n",
                        "```",
                    });
                    await ctx.Channel.SendMessageAsync(error_mes);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Source + ": `" + e.Message + "`");
                }
            }
        }

        [Command("new"), RequirePermissions(Permissions.ManageChannels), Description(NewCmdDescription)]
        public async Task NewVoiceChannelAsync(CommandContext ctx, 
            [Description(RemainingTextNewCmdDescription),RemainingText] string txt)
        {
            DiscordGuild guild = ctx.Guild;

            if (guild != null)
            {
                try
                {
                    VoiceCommandConverter.ResultConverterNewCmd resultConverter = VoiceCommandConverter.NewVoiceChannelCmdConverter(txt);

                    DiscordChannel result = await guild.CreateVoiceChannelAsync(
                        resultConverter.name, bitrate: resultConverter.bitrates, user_limit: resultConverter.user_limit, qualityMode: VideoQualityMode.Auto,
                        reason: "Request create new voice channel by " + ctx.Member.Username + " , from QuickServer bot"
                    );
                }
                catch (Bad​Request​Exception e)
                {
                    string error_mes = string.Concat(new string[] {
                        "```json \n",
                        e.WebResponse.ResponseCode.ToString()+"\nDetail: ",
                        e.WebResponse.Response,
                        "```",
                    });
                    await ctx.Channel.SendMessageAsync(error_mes);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Source+": `"+e.Message+"`");
                }
            }
        }
        #endregion

        #region allow cmd
        [Command("allow")]
        public async Task AllowCommandAsync(CommandContext ctx, 
            [Description("Member need to allow to join voice")] DiscordMember member) {
            if (ctx.Member.VoiceState != null)
            {
                DiscordChannel channel = ctx.Member.VoiceState.Channel;
                bool yesss = false;
                Permissions allow = Permissions.None;
                Permissions deny = Permissions.None;
                foreach (DiscordOverwrite overwrite in channel.PermissionOverwrites) {
                    if (overwrite.Type == OverwriteType.Member) {
                        DiscordMember mem_overwrite = await overwrite.GetMemberAsync();
                        if( mem_overwrite.Id == member.Id)
                        {
                            allow = overwrite.Allowed;
                            deny = overwrite.Denied;
                            yesss = true;
                        }
                    }
                }

                if (yesss)
                {
                    allow = PermissionChecker.GrantByList(allow, new List<Permissions> { Permissions.AccessChannels, Permissions.Speak });
                    await channel.AddOverwriteAsync(member, allow:allow, deny:deny);
                    await ctx.RespondAsync("Allowed " + member.Mention + " your voice channel");
                }
                else
                {
                    await channel.AddOverwriteAsync(member, allow: PermissionMethods.Grant(Permissions.AccessChannels, Permissions.Speak));
                    await ctx.RespondAsync("Allowed " + member.Mention + " your voice channel");
                }
            }
            else
            {
                await ctx.RespondAsync("You aren't in any voice channel");
            }
        }
        #endregion
    }
}
