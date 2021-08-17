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
        /// <param name="text"></param>
        /// <returns>new ResultConverterNewCmd</returns>
        /// <exception cref="ArgumentException"></exception>
        internal static ResultConverterNewCmd NewVoiceChannelCmdConverter(string text)
        {
            try
            {
                string[] first = text.Split('`').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                string name = first[0];
                if (first.Length == 1)
                {
                    return new ResultConverterNewCmd { name = name };
                }
                string[] second = first[1].Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                int bitrate = Int32.Parse(second[0]);

                // Understand by yourself :) Min bitrate are 8kbps = 8000 bps
                bitrate = bitrate >= 8000 ? bitrate 
                    : bitrate >= 800 ? bitrate * 10 
                    : bitrate >= 80 ? bitrate * 100 
                    : bitrate > 8 ? bitrate * 1000 
                    : throw new ArgumentException("bitrate are invalid");

                if (second.Length == 1)
                {
                    return new ResultConverterNewCmd { name = name, bitrates = bitrate };
                }
                return new ResultConverterNewCmd { name = name, bitrates = bitrate, user_limit = Int32.Parse(second[1]) };
            }
            catch (Exception e) {
                throw new ArgumentException(e.Message);
            }
        }
        internal class ResultConverterNewCmd {
            public string name { get; internal set; }
            public int bitrates = 64;
            public int user_limit = 0; // No limit
        }
    }

}
