using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    public static class Logger
    {
        public static void Log(object text)
        {
            string output = string.Format("{0:HH:mm:ss.fff}         {1}", DateTime.Now, text);
            Console.WriteLine(output);
        }
    }
}
