using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordBot.DBCommand.Channel
{
    public class ChannelCommandConverter
    {
        internal static ResultConverterNewCmd NewCommandConverter(string name, int slowtime, string isNSFW)
        {
            string remove_specialcharacter() { 
                string[] replaceables = new[] { "+", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "~", "*", "?", ":", "\\", "\"" };
                string rxString = string.Join("|", replaceables.Select(s => Regex.Escape(s)));
                return Regex.Replace(name, rxString, "");
            }
            name = remove_specialcharacter();

            ResultConverterNewCmd resultConverter = new ResultConverterNewCmd();

            resultConverter.name = name;

            resultConverter.isNsfw = new string[] { "yes", "enable", "accept" }.Contains(isNSFW) ? true :
                new string[] {"no", "enable", "deny" }.Contains(isNSFW) ? false :
                throw new ArgumentException("invalid isNSFW value");

            resultConverter.slowtime = slowtime <= 21600 ? slowtime > 0 ? slowtime : 0 : 21600;
            return resultConverter;
        }

        internal class ResultConverterNewCmd {
            public string name { get; internal set; }
            public bool isNsfw { get; internal set; }
            public int slowtime { get; internal set; }
        }
    }
}
