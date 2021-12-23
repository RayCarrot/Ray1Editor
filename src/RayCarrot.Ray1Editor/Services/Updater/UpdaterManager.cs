using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// Manages application updates
/// </summary>
public abstract class UpdaterManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Checks for updates
    /// </summary>
    /// <param name="forceUpdate">Indicates if the latest available version should be returned even if it's not newer than the current version</param>
    /// <param name="includeBeta">Indicates if beta updates should be included in the check</param>
    /// <returns>The result</returns>
    public async Task<UpdaterCheckResult> CheckAsync(bool forceUpdate, bool includeBeta)
    {
        Logger.Log(LogLevel.Info, "Updates are being checked for");

        string errorMessage = "Unknown error";
        Exception exception = null;
        JObject manifest = null;
        Version latestFoundVersion;

        try
        {
            // Create the web client
            using var wc = new HttpClient();

            // Download the manifest
            var result = await wc.GetStringAsync(ManifestURL);

            // Parse the manifest
            manifest = JObject.Parse(result);
        }
        catch (WebException ex)
        {
            exception = ex;
            Logger.Log(LogLevel.Warn, ex, "Getting server manifest");
            errorMessage = "A connection could not be established to the server";
        }
        catch (JsonReaderException ex)
        {
            exception = ex;
            Logger.Log(LogLevel.Error, ex, "Parsing server manifest");
            errorMessage = "The information from the server was not valid";
        }
        catch (Exception ex)
        {
            exception = ex;
            Logger.Log(LogLevel.Error, ex, "Getting server manifest");
            errorMessage = "An unknown error occurred while connecting to the server";
        }

        // Return the error if the manifest was not retrieved
        if (manifest == null)
            return new UpdaterCheckResult(errorMessage, exception);

        // Flag indicating if the current update is a beta update
        bool isBetaUpdate = false;

        Logger.Log(LogLevel.Info, "The update manifest was retrieved");

        try
        {
            // Get the latest version
            var latestVersion = GetLatestVersion(manifest, false);

            // Get the latest beta version
            var latestBetaVersion = GetLatestVersion(manifest, true);

            // If a new update is not available...
            if (CurrentVersion >= latestVersion)
            {
                // If we are forcing new updates, download the latest update
                if (forceUpdate)
                {
                    // If we are including beta updates, check if it's newer
                    if (includeBeta)
                    {
                        // If it's newer, get the beta update
                        if (latestBetaVersion > latestVersion)
                            isBetaUpdate = true;
                    }
                }
                // If we are not forcing updates, check if a newer beta version is available
                else
                {
                    // Check if a newer beta version is available, if set to do so
                    if (includeBeta && CurrentVersion < latestBetaVersion)
                    {
                        isBetaUpdate = true;
                    }
                    else
                    { 
                        Logger.Log(LogLevel.Info, "The latest version is installed");

                        // Return the result
                        return new UpdaterCheckResult();
                    }
                }
            }

            latestFoundVersion = isBetaUpdate ? latestBetaVersion : latestVersion;

            Logger.Log(LogLevel.Info, "A new version ({0}) is available", latestFoundVersion);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex, "Getting assembly version from server manifest {0}", manifest);

            return new UpdaterCheckResult("The server manifest could not be read", ex);
        }

        // Get the download URL
        string downloadURL;

        try
        {
            downloadURL = GetDownloadURL(manifest, isBetaUpdate);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex, "Getting download URL from server manifest {0}", manifest);

            return new UpdaterCheckResult("The server manifest could not be read", ex);
        }

        // Get the display news
        string displayNews = null;

        try
        {
            // Get the update news
            displayNews = GetDisplayNews(manifest, isBetaUpdate);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex, "Getting update news from server manifest {0}", manifest);
        }

        // Return the result
        return new UpdaterCheckResult(latestFoundVersion, downloadURL, displayNews ?? "Error getting news", isBetaUpdate);
    }

    /// <summary>
    /// Updates the application
    /// </summary>
    /// <param name="result">The updater check result to use when updating</param>
    /// <param name="asAdmin">Indicates if the updater should run as admin</param>
    /// <returns>A value indicating if the operation succeeded</returns>
    public bool Update(UpdaterCheckResult result, bool asAdmin)
    {
        var updateFilePath = R1EServices.App.Path_UpdaterFile;

        try
        {
            // Deploy the updater
            using (var file = File.Create(updateFilePath))
            {
                using (var updateAsset = Assets.GetAsset("Assets/Deployable/RayCarrot.Ray1Editor.Updater.exe"))
                {
                    updateAsset.CopyTo(file);
                }
            }

            Logger.Log(LogLevel.Info, "The updater was created");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex, "Writing updater to temp path");

            R1EServices.UI.DisplayMessage($"The updater could not be created. To manually download the new version, go to {R1EServices.App.Url_Ray1EditorHome} and download the latest version from there.", "Error creating updater", DialogMessageType.Error);
                
            return false;
        }

        // Launch the updater and capture the process
        using var updateProcess = R1EServices.File.LaunchFile(updateFilePath, asAdmin,
            // Arg 1: Program path
            $"\"{Process.GetCurrentProcess().MainModule?.FileName}\" " +
            // Arg 2: Dark mode
            $"{R1EServices.App.UserData.Theme_Dark} " +
            // Arg 3: Update URL
            $"\"{result.DownloadURL}\"");

        // Make sure we have a valid process
        if (updateProcess == null)
        {
            R1EServices.UI.DisplayMessage($"The updater could not be launched. To manually download the new version, go to {R1EServices.App.Url_Ray1EditorHome} and download the latest version from there.", "Error updating", DialogMessageType.Error);

            return false;
        }

        // Shut down the app
        App.Current.Shutdown();

        return true;
    }

    /// <summary>
    /// Gets the latest version from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The latest version</returns>
    protected abstract Version GetLatestVersion(JObject manifest, bool isBeta);

    /// <summary>
    /// Gets the display news from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The display news</returns>
    protected abstract string GetDisplayNews(JObject manifest, bool isBeta);

    /// <summary>
    /// Gets the download URL from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The download URL</returns>
    protected abstract string GetDownloadURL(JObject manifest, bool isBeta);

    /// <summary>
    /// The current version of the application
    /// </summary>
    protected abstract Version CurrentVersion { get; }

    /// <summary>
    /// The fallback URL to display to the user in case of an error
    /// </summary>
    protected abstract string UserFallbackURL { get; }

    /// <summary>
    /// The manifest URL
    /// </summary>
    protected abstract string ManifestURL { get; }
}