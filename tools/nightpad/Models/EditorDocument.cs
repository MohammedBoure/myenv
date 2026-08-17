using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace NightPad.Models;

/// <summary>
/// Represents an open tab/document in NightPad.
/// </summary>
public class EditorDocument : INotifyPropertyChanged
{
    private string? _filePath;
    private string _title = "Untitled";
    private bool _isModified;
    private string _syntaxName = "Plain Text";
    private string _encodingName = "UTF-8";
    private string _eolName = "CRLF";
    private int _lineCount = 1;
    private int _wordCount = 0;
    private int _charCount = 0;
    private int _caretLine = 1;
    private int _caretColumn = 1;
    private int _selectionLength = 0;
    private int _selectionLines = 0;

    public EditorDocument(string? filePath = null, string? initialContent = null, string? title = null)
    {
        _filePath = filePath;
        Document = new TextDocument(initialContent ?? string.Empty);
        
        if (!string.IsNullOrEmpty(filePath))
        {
            _title = Path.GetFileName(filePath);
        }
        else if (!string.IsNullOrEmpty(title))
        {
            _title = title;
        }

        UpdateStatistics();
    }

    public TextDocument Document { get; }

    public TextEditor? Editor { get; set; }

    public string? FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath != value)
            {
                _filePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FileName));
                UpdateTitle();
            }
        }
    }

    public string FileName => string.IsNullOrEmpty(_filePath) ? _title : Path.GetFileName(_filePath);

    public string DisplayTitle => _isModified ? $"{FileName} *" : FileName;

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (_isModified != value)
            {
                _isModified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public string SyntaxName
    {
        get => _syntaxName;
        set
        {
            if (_syntaxName != value)
            {
                _syntaxName = value;
                OnPropertyChanged();
            }
        }
    }

    public string EncodingName
    {
        get => _encodingName;
        set
        {
            if (_encodingName != value)
            {
                _encodingName = value;
                OnPropertyChanged();
            }
        }
    }

    public string EolName
    {
        get => _eolName;
        set
        {
            if (_eolName != value)
            {
                _eolName = value;
                OnPropertyChanged();
            }
        }
    }

    public int LineCount
    {
        get => _lineCount;
        set
        {
            if (_lineCount != value)
            {
                _lineCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int WordCount
    {
        get => _wordCount;
        set
        {
            if (_wordCount != value)
            {
                _wordCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int CharCount
    {
        get => _charCount;
        set
        {
            if (_charCount != value)
            {
                _charCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int CaretLine
    {
        get => _caretLine;
        set
        {
            if (_caretLine != value)
            {
                _caretLine = value;
                OnPropertyChanged();
            }
        }
    }

    public int CaretColumn
    {
        get => _caretColumn;
        set
        {
            if (_caretColumn != value)
            {
                _caretColumn = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectionLength
    {
        get => _selectionLength;
        set
        {
            if (_selectionLength != value)
            {
                _selectionLength = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectionLines
    {
        get => _selectionLines;
        set
        {
            if (_selectionLines != value)
            {
                _selectionLines = value;
                OnPropertyChanged();
            }
        }
    }

    public void UpdateTitle()
    {
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(DisplayTitle));
    }

    public void UpdateStatistics()
    {
        string text = Document.Text;
        CharCount = text.Length;
        LineCount = Document.LineCount;

        // Word count calculation
        int words = 0;
        bool inWord = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                words++;
            }
        }
        WordCount = words;

        // Detect EOL
        if (text.Contains("\r\n"))
            EolName = "CRLF";
        else if (text.Contains("\n"))
            EolName = "LF";
        else
            EolName = "CRLF";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
