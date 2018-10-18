using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BloodBot
{
    public static class Config
    {
        public static Dictionary<string, string> Data = new Dictionary<string, string>();

        public struct ConfigKeys
        {
            public static string DiscordToken = "DiscordToken";
            public static string ConnectionString = "ConnectionString";
        }

        static Config()
        {
            if (File.Exists(@".\Config.txt"))
            {
                string line;
                using (StreamReader reader = new StreamReader(@".\Config.txt"))
                {
                    line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] substrings = line.Split('#');
                        if (!Data.ContainsKey(substrings[0]))
                        {
                            Data.Add(substrings[0], substrings[1]);
                        }
                    }
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(@".\Config.txt"))
                {
                    writer.WriteLine("DiscordToken=");
                    writer.WriteLine("ConnectionString=");
                }
                Logger.Log("Fill in the config file and try again");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
