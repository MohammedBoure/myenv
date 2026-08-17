using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace NightPad.Models;

/// <summary>
/// Represents a file or directory node in the NightPad Explorer sidebar.
/// </summary>
public class FileNode : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isLoaded;
    private string _name;
    private string _icon;

    public FileNode(string fullPath, bool isDirectory)
    {
        FullPath = fullPath;
        IsDirectory = isDirectory;
        _name = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(_name))
        {
            _name = fullPath; // For root drives like C:\
        }
        _icon = DetermineIcon(fullPath, isDirectory);
        Children = new ObservableCollection<FileNode>();

        if (isDirectory)
        {
            // Add dummy child for lazy loading
            Children.Add(new FileNode(string.Empty, false) { Name = "Loading..." });
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public string FullPath { get; set; }

    public bool IsDirectory { get; set; }

    public string Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<FileNode> Children { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
                if (_isExpanded && !_isLoaded && IsDirectory)
                {
                    LoadChildren();
                }
            }
        }
    }

    public void LoadChildren()
    {
        if (_isLoaded || !IsDirectory || string.IsNullOrEmpty(FullPath) || !Directory.Exists(FullPath))
            return;

        try
        {
            Children.Clear();

            var dirInfo = new DirectoryInfo(FullPath);

            // 1. Add directories
            foreach (var dir in dirInfo.GetDirectories())
            {
                // Skip hidden/system directories unless needed
                if ((dir.Attributes & FileAttributes.Hidden) != 0 && dir.Name.StartsWith("."))
                    continue;

                Children.Add(new FileNode(dir.FullName, true));
            }

            // 2. Add files
            foreach (var file in dirInfo.GetFiles())
            {
                if ((file.Attributes & FileAttributes.Hidden) != 0 && file.Name.StartsWith("."))
                    continue;

                Children.Add(new FileNode(file.FullName, false));
            }

            _isLoaded = true;
        }
        catch
        {
            Children.Clear();
            _isLoaded = true;
        }
    }

    private static string DetermineIcon(string path, bool isDir)
    {
        if (isDir) return "📁";

        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".py" or ".pyw" => "🐍",
            ".cs" => "🔷",
            ".js" or ".jsx" or ".mjs" => "🟨",
            ".ts" or ".tsx" => "🔷",
            ".json" or ".jsonc" => "🧾",
            ".md" or ".markdown" => "📝",
            ".html" or ".htm" => "🌐",
            ".css" or ".scss" or ".sass" or ".less" => "🎨",
            ".ps1" or ".psm1" or ".psd1" => "⚡",
            ".cmd" or ".bat" => "⚙️",
            ".sql" => "🗄️",
            ".yaml" or ".yml" => "📋",
            ".xml" or ".xaml" or ".svg" or ".csproj" => "📦",
            ".cpp" or ".c" or ".h" or ".hpp" => "⚙️",
            ".rs" => "🦀",
            ".go" => "🐹",
            ".java" => "☕",
            ".php" => "🐘",
            ".sh" or ".bash" or ".zsh" => "🐚",
            ".txt" or ".log" or ".env" or ".ini" or ".cfg" => "📄",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".ico" => "🖼️",
            ".pdf" => "📕",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "🗜️",
            _ => "📄"
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
