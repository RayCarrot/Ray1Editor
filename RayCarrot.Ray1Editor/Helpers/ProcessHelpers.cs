using System;
using System.Diagnostics;
using System.IO;
using NLog;

namespace RayCarrot.Ray1Editor
{
    public static class ProcessHelpers
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string GetStringAsPathArg(string filePath) => $"\"{filePath.Replace('/', '\\')}\"";

        public static void RunProcess(string filePath, string[] args, string workingDir = null, bool waitForExit = true, bool logInfo = true)
        {
            // Create the process and dispose when finished
            using var p = new Process();

            // Set the start info
            p.StartInfo = new ProcessStartInfo(filePath, String.Join(" ", args))
            {
                UseShellExecute = !logInfo,
                RedirectStandardOutput = logInfo,
                WorkingDirectory = workingDir ?? Path.GetDirectoryName(filePath)
            };

            if (logInfo)
                Logger.Log(LogLevel.Info, $"Starting process {p.StartInfo.FileName} with arguments: {p.StartInfo.Arguments}");

            p.Start();

            if (waitForExit)
            {
                p.WaitForExit();

                if (logInfo)
                    Logger.Log(LogLevel.Info, $"Process output: {p.StandardOutput.ReadToEnd()}");
            }
        }
    }
}