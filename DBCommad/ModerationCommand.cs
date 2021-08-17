using DiscordBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.DBCommad
{
    public class ModerationCommand : BaseCommandModule
    {
        [Command("mute"), Description("Assign member THE FIRST ROLE HAS NAME `Muted`\nWARNING: MUTE = MUTE FOREVER!")]
        public async Task MuteMemberAsync(CommandContext ctx, [Description("")] DiscordMember member) 
        {
            DiscordGuild guild = ctx.Guild;
            DiscordRole roleMuted = null;
            foreach (KeyValuePair<ulong, DiscordRole> item in guild.Roles)
            {
                if (item.Value.Name == "Muted")
                {
                    roleMuted = item.Value;
                }
            }

            
        }
    }
}
