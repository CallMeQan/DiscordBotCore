using DiscordBot.Utils;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DBCommand.Channel
{
    public class VoiceCommandConverter
    {
        internal static ResultConverterNewCmd NewVoiceChannelCmdConverter(string text)
        {
            string name = text.Split('"')[0] ;

            return new ResultConverterNewCmd { name = name };
        }
    }

    internal class ResultConverterNewCmd {
        public string name { get; internal set; }
        public int bitrates = 64;
        public int user_limit = 0; // No limit
    }
}
