using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NightPad.Services;

/// <summary>
/// Service providing fast terminal-style path resolution, auto-completion, and directory utilities.
/// </summary>
public static class PathCompletionService
{
    public record PresetDirectory(string Name, string Shortcut, string Path);

    /// <summary>
    /// Returns quick access directory presets.
    /// </summary>
    public static List<PresetDirectory> GetPresets()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var myenv = Path.Combine(docs, "myenv");

        var list = new List<PresetDirectory>
        {
            new("Current Dir", "F1", Environment.CurrentDirectory),
            new("Documents", "F2", docs),
            new("Desktop", "F3", desktop),
        };

        if (Directory.Exists(myenv))
        {
            list.Add(new("MyEnv", "F4", myenv));
        }

        var downloads = Path.Combine(userProfile, "Downloads");
        if (Directory.Exists(downloads))
        {
            list.Add(new("Downloads", "F5", downloads));
        }

        return list;
    }

    /// <summary>
    /// Normalizes and resolves user input into an absolute path.
    /// Supports '~' (home directory), environment variables (%USERPROFILE%), and relative paths.
    /// </summary>
    public static string ResolvePath(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            return string.Empty;

        string path = rawPath.Trim();

        // Expand environment variables (e.g. %USERPROFILE%)
        path = Environment.ExpandEnvironmentVariables(path);

        // Expand '~' to user profile directory
        if (path.StartsWith("~\\") || path.StartsWith("~/"))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(userProfile, path[2..]);
        }
        else if (path == "~")
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        try
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
            }
            else
            {
                path = Path.GetFullPath(path);
            }
        }
        catch
        {
            // Return raw path if invalid characters are present
        }

        return path;
    }

    /// <summary>
    /// Gets auto-completion matches for a partial path.
    /// </summary>
    public static List<string> GetCompletions(string input)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
            return results;

        try
        {
            string expanded = Environment.ExpandEnvironmentVariables(input.Trim());
            if (expanded.StartsWith("~\\") || expanded.StartsWith("~/"))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                expanded = Path.Combine(userProfile, expanded[2..]);
            }

            string? dirPart = Path.GetDirectoryName(expanded);
            string filePrefix = Path.GetFileName(expanded);

            string searchDir;
            if (string.IsNullOrEmpty(dirPart))
            {
                searchDir = Environment.CurrentDirectory;
            }
            else if (!Path.IsPathRooted(dirPart))
            {
                searchDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, dirPart));
            }
            else
            {
                searchDir = dirPart;
            }

            if (!Directory.Exists(searchDir))
                return results;

            // Search matching directories first, then matching files
            var dirs = Directory.GetDirectories(searchDir, filePrefix + "*")
                                .OrderBy(d => d)
                                .Take(15);

            foreach (var d in dirs)
            {
                results.Add(d + Path.DirectorySeparatorChar);
            }

            var files = Directory.GetFiles(searchDir, filePrefix + "*")
                                 .OrderBy(f => f)
                                 .Take(15);

            foreach (var f in files)
            {
                results.Add(f);
            }
        }
        catch
        {
            // Silently return empty list on invalid input / permission issues
        }

        return results;
    }

    /// <summary>
    /// Ensures that the parent directory of a file path exists, creating it if necessary.
    /// </summary>
    public static void EnsureDirectoryExists(string fullFilePath)
    {
        string? dir = Path.GetDirectoryName(fullFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
