using System;
using System.Diagnostics;
using System.IO;
using NLog;

namespace RayCarrot.Ray1Editor;

public class FileManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void OpenURL(string url)
    {
        try
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
            {
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex, "Opening URL {0}", url);
        }
    }

    public Process LaunchFile(string file, bool asAdmin = false, string arguments = null, string wd = null)
    {
        try
        {
            // Create the process start info
            ProcessStartInfo info = new ProcessStartInfo
            {
                // Set the file path
                FileName = file,

                // Set to working directory to the parent directory if not otherwise specified
                WorkingDirectory = wd ?? Path.GetDirectoryName(file),

                UseShellExecute = true
            };

            // Set arguments if specified
            if (arguments != null)
                info.Arguments = arguments;

            // Set to run as admin if specified
            if (asAdmin)
                info.Verb = "runas";

            // Start the process and get the process
            var p = Process.Start(info);

            Logger.Log(LogLevel.Info, "The file {0} launched with the arguments: {1}", file, arguments);

            // Return the process
            return p;
        }
        catch (FileNotFoundException ex)
        {
            Logger.Log(LogLevel.Warn, ex, "Launching file", file);

            R1EServices.UI.DisplayMessage($"The specified file could not be found: {file}", "File not found", DialogMessageType.Error);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, ex, "Launching file", file);

            R1EServices.UI.DisplayMessage($"An error occurred when attempting to run {file}", "Error opening file", DialogMessageType.Error);
        }

        // Return null if the process could not launch
        return null;
    }

    public void OpenExplorerPath(string path)
    {
        try
        {
            if (File.Exists(path))
                Process.Start("explorer.exe", "/select, \"" + path + "\"")?.Dispose();
            else if (Directory.Exists(path))
                Process.Start("explorer.exe", path)?.Dispose();

            Logger.Log(LogLevel.Trace, "Opened path in explorer");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, ex, "Opening explorer path", path);

        }
    }

    public bool CheckFileWriteAccess(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Trace, ex, "Checking for file write access");
            return false;
        }
    }
}