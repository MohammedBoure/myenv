using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NightPad.Services;

/// <summary>
/// Provides text manipulation, formatting, conversion, and transformation utilities.
/// </summary>
public static class TextTransformService
{
    public static string ToUpperCase(string input) => input.ToUpperInvariant();

    public static string ToLowerCase(string input) => input.ToLowerInvariant();

    public static string ToTitleCase(string input)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    public static string ToInvertCase(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (char.IsUpper(c))
                sb.Append(char.ToLower(c));
            else if (char.IsLower(c))
                sb.Append(char.ToUpper(c));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    public static string SortLines(string input, bool descending)
    {
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sorted = descending
            ? lines.OrderByDescending(l => l, StringComparer.OrdinalIgnoreCase)
            : lines.OrderBy(l => l, StringComparer.OrdinalIgnoreCase);

        string separator = input.Contains("\r\n") ? "\r\n" : "\n";
        return string.Join(separator, sorted);
    }

    public static string RemoveDuplicateLines(string input)
    {
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var unique = lines.Distinct();
        string separator = input.Contains("\r\n") ? "\r\n" : "\n";
        return string.Join(separator, unique);
    }

    public static string RemoveEmptyLines(string input)
    {
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l));
        string separator = input.Contains("\r\n") ? "\r\n" : "\n";
        return string.Join(separator, nonEmpty);
    }

    public static string TrimTrailingWhitespace(string input)
    {
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var trimmed = lines.Select(l => l.TrimEnd());
        string separator = input.Contains("\r\n") ? "\r\n" : "\n";
        return string.Join(separator, trimmed);
    }

    public static string FormatJson(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid JSON: {ex.Message}", ex);
        }
    }

    public static string MinifyJson(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid JSON: {ex.Message}", ex);
        }
    }

    public static string ToBase64(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(string input)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(input.Trim());
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid Base64 string: {ex.Message}", ex);
        }
    }

    public static string ToUrlEncoded(string input)
    {
        return Uri.EscapeDataString(input);
    }

    public static string FromUrlEncoded(string input)
    {
        return Uri.UnescapeDataString(input);
    }

    public static string GetCurrentTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
