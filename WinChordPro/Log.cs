using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysIO = System.IO;

namespace WinChordPro
{
    class Log
    {
        public static String logFile = @"C:\Windows\Temp\WinChordProLog.txt";

        public void initialize()
        {
            string date = DateTime.Now.ToShortDateString();
            SysIO.File.WriteAllText(logFile, date + " ************************\r\n");

        }

        public void log(string logText)
        {
            string time = DateTime.Now.ToLongTimeString();

            SysIO.File.AppendAllText(logFile,  time + " " + logText + "\r\n");
        }
    }
}
