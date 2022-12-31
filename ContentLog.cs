using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microcosm
{
    public static class ContentLog
    {
        static ContentLog()
        {
            ContentLog.Lines = new List<string>();
        }

        public static List<string> Lines { get; private set; }

        public static TimeSpan GetUptime()
        {
            Process process = Process.GetCurrentProcess();
            return DateTime.Now - process.StartTime;
        }

        public static void Log(StaticFileResponseContext context)
        {
            string fileName = context.File.Name;
            string logLine = $"{DateTime.UtcNow}, {context.Context.Connection.RemoteIpAddress}, {fileName}";
            ContentLog.Lines.Insert(0, logLine);
            while (ContentLog.Lines.Count > 1000)
            {
                ContentLog.Lines.RemoveAt(1000);
            }
        }
    }
}
