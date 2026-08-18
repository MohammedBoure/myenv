using System;
using System.Text.RegularExpressions;

namespace NightPad.Services;

/// <summary>
/// Service providing Arabic language utilities, bidirectional text flow detection, and Unicode word counting.
/// </summary>
public static partial class ArabicTextService
{
    // Matches Arabic Unicode blocks: Basic Arabic, Arabic Supplement, Arabic Extended-A/B, Presentation Forms
    [GeneratedRegex(@"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]")]
    private static partial Regex ArabicRegex();

    // Matches Unicode words across any language (Arabic, Latin, Cyrillic, CJK, etc.)
    [GeneratedRegex(@"[\p{L}\p{N}_]+")]
    private static partial Regex UnicodeWordRegex();

    /// <summary>
    /// Checks if the text contains any Arabic characters.
    /// </summary>
    public static bool ContainsArabic(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return ArabicRegex().IsMatch(text);
    }

    /// <summary>
    /// Determines whether the primary direction of the text should be Right-To-Left (RTL).
    /// Evaluates the first significant non-whitespace characters.
    /// </summary>
    public static bool ShouldBeRightToLeft(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Check first 100 characters to determine orientation
        string sample = text.Length > 100 ? text[..100] : text;
        int arabicCount = 0;
        int latinCount = 0;

        foreach (char c in sample)
        {
            if (c >= '\u0600' && c <= '\u06FF' || c >= '\u0750' && c <= '\u077F' || c >= '\u08A0' && c <= '\u08FF')
            {
                arabicCount++;
            }
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                latinCount++;
            }
        }

        return arabicCount > latinCount;
    }

    /// <summary>
    /// Accurately counts words in Unicode text, fully supporting Arabic and multilingual texts.
    /// </summary>
    public static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return UnicodeWordRegex().Matches(text).Count;
    }
}
