using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Services
{
    public class LogService
    {
        readonly string logDirectory;
        private string logPatch;
        public LogService()
        {
            logDirectory = CreateDirLog();
        }

        public void CreateLogFile()
        {
            var timeStr = $"{DateTime.Now.Hour:00}_{DateTime.Now.Minute:00}_{DateTime.Now.Second:00}";
            logPatch = $"{logDirectory}\\{timeStr}.log";

            if (!File.Exists(logPatch))
            {
                using (var fs = File.Create(logPatch)) { }
            }
        }

        public void Write(string value, bool withTime = false)
        {
            try
            {
                if (withTime)
                {
                    value = $"{DateTime.Now} {value}";
                }
                using (StreamWriter sw = new StreamWriter(logPatch, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(value);
                }
            }
            catch (Exception e)
            {
                //
            }
        }

        private string GetCurrentDirLogs()
        {
            return $"{Directory.GetCurrentDirectory()}\\logs";
        }

        private string CreateDirLog()
        {
            var result = $"{GetCurrentDirLogs()}\\{DateTime.Now.ToShortDateString()}";
            DirectoryInfo dirInfo = new DirectoryInfo(result);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            return result;
        }
    }
}
