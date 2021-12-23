using System;
using System.IO;
using NLog;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// A temporary file
/// </summary>
public sealed class TempFile : IDisposable
{
    /// <summary>
    /// Creates a new temporary file
    /// </summary>
    public TempFile()
    {
        // Get the temp path and create the file
        TempPath = Path.GetTempFileName();

        // Get the file info
        var info = new FileInfo(TempPath);

        // Set the attribute to temporary
        info.Attributes |= FileAttributes.Temporary;

        Logger.Log(LogLevel.Info, $"A new temp file has been created under {TempPath}");
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The path of the temporary file
    /// </summary>
    public string TempPath { get; }

    public Stream OpenRead() => File.OpenRead(TempPath);

    /// <summary>
    /// Removes the temporary file
    /// </summary>
    public void Dispose()
    {
        try
        {
            // Delete the temp file
            File.Delete(TempPath);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Couldn't delete temp file at {TempPath} with error {ex}");
        }
    }
}