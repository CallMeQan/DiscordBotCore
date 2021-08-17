using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.Utils
{
    public class PermissionChecker
    {
        /// <summary>
        /// <code>
        /// string input = "Manage channel, Manage Server"
        /// PermissionChecker.ConvertPerms(input);
        /// </code>
        /// </summary>
        /// <param name="input">a string of permission</param>
        /// <returns>Permission converted</returns>
        public static Permissions ConvertPerms(string input)
        {
            string[] remove_whitespace_begin_and_last(string[] a)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = a[i].Trim();
                }
                return a;
            }
            Permissions return_ = new Permissions();
            string[] input_converted = remove_whitespace_begin_and_last(input.ToLower().Split(','));

            foreach (KeyValuePair<Permissions, string[]> perm in DiscordBot.Utils.PermissionChecker.perms)
            {
                foreach (string item in perm.Value)
                {
                    if (input_converted.Contains(item))
                        if (!return_.HasPermission(perm.Key))
                            return_ = PermissionMethods.Grant(return_, perm.Key);
                }
            }
            return return_;
        }

        public static Permissions GrantByList(Permissions orgin_perm, List<Permissions> perms_grant) {
            perms_grant.ForEach(perm => {
                orgin_perm = PermissionMethods.Grant(orgin_perm, perm);
            });
            return orgin_perm;
        }

        public static Permissions GrantByString(Permissions orgin_perm, string input) {
            return PermissionMethods.Grant(orgin_perm, ConvertPerms(input));
        }

        public static Dictionary<Permissions, string[]> perms = new Dictionary<Permissions, string[]>() {
            { Permissions.AccessChannels, new string[]{ 
                "access", "accesschannel", "access channel", "access_channel"
            } },
            { Permissions.Administrator, new string[]{ 
                "admin"
            } },
            { Permissions.BanMembers, new string[]{
                "ban","ban member","banmember","ban mem" 
            } },
            { Permissions.ChangeNickname, new string[]{ 
                "nickname", "change nickname", "changenickname"
            } },
            { Permissions.DeafenMembers, new string[]{ 
                "deafen"
            } },
            { Permissions.CreateInstantInvite, new string[]{ 
                "invite", "create invite", "create invite link"
            } },
            { Permissions.EmbedLinks, new string[]{ 
                "embed"
            } },
            { Permissions.ManageChannels, new string[]{
                "manage channel", "managechannel"
            } },
            { Permissions.ManageEmojis, new string[]{ 
                "manage emoji", "manageemoji"
            } },
            { Permissions.ManageGuild, new string[]{ 
                "manage guild", "manageguild", "manage server", "manageserver"
            } },
            { Permissions.ManageMessages, new string[]{
                "managemessage", "managemsg", "manage message", "manage msg", "managemes", "manage mes"
            } },
            { Permissions.ManageRoles, new string[]{
                "manage role", "managerole"
            } },
            { Permissions.ManageThreads, new string[]{ 
                "manage thread", "managethread"
            } },
            { Permissions.ManageWebhooks, new string[]{ 
                "managewebhook", "managewebhook"
            } },
            { Permissions.MoveMembers, new string[]{ 
                "move", "movevoice", "movevc", "move member", "movemember", "move mem", "movemem"
            } },
            { Permissions.MuteMembers, new string[]{ 
                "mute"
            } },
            { Permissions.None, new string[]{ 
                "none", "empty"
            } },
            { Permissions.PrioritySpeaker, new string[]{
                "priority speak", "priorityspeak", "first speak" 
            } },
            { Permissions.ReadMessageHistory, new string[]{ 
                "read message history", "readmessagehistory", "read mes history", "readmeshistory" 
            } },
            { Permissions.RequestToSpeak, new string[]{
                "requestspeak", "request speak" 
            } },
            { Permissions.SendMessages, new string[]{
                "send message", "send mes", "sendmes", "sendmessage"
            } },
            { Permissions.SendTtsMessages, new string[]{ 
                "sendtts", "send tts"
            } },
            { Permissions.Speak, new string[]{ 
                "speak"
            } },
            { Permissions.Stream, new string[]{
                "stream", "livestream"
            } },
            { Permissions.UseExternalEmojis, new string[]{
                "external emoji", "externalemoji" 
            } },
            { Permissions.UseExternalStickers, new string[]{
                "external sticker", "externalsticker"
            } },
            { Permissions.UseVoiceDetection, new string[]{ 
                "voicedetect"
            } },
            { Permissions.UseSlashCommands, new string[]{
                "slashcommand", "slashcmd"
            } },
            { Permissions.UsePublicThreads, new string[]{
                "publicthread", "public thread"
            } },
            { Permissions.UsePrivateThreads, new string[]{
                "privatethread", "private thread"
            } },
            { Permissions.ViewAuditLog, new string[]{ 
                "auditlog", "audit log"
            } },
            { Permissions.AttachFiles, new string[]{ 
                "attachfile", "attach file", "file"
            } },
            { Permissions.AddReactions, new string[]{
                "reaction", "add reaction", "addreaction"
            } },
            { Permissions.KickMembers, new string[]{
                "kick"
            } }
        };
    }
}
