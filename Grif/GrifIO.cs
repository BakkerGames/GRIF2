using System.Text;
using static Grif.Common;

namespace Grif;

public static class GrifIO
{
    public static void WriteGrif(string filePath, List<GrodItem> items)
    {
        using var writer = new StreamWriter(filePath);
        foreach (var item in items)
        {
            writer.WriteLine(item.Key);
            if (item.Value != null && item.Value.StartsWith('@'))
            {
                // If the value starts with '@', it is a script
                writer.WriteLine(Dags.PrettyScript(item.Value, true));
            }
            else
            {
                // Otherwise, write the value without quotes
                var value = item.Value;
                value ??= NULL;
                while (value.Length > 2 && value.Contains(NL, OIC))
                {
                    var pos = value.IndexOf(NL, OIC);
                    writer.WriteLine($"\t{value[..(pos + 2)]}");
                    value = value[(pos + 2)..]; // Skip the escape sequence
                }
                if (value.Length > 0)
                {
                    writer.WriteLine($"\t{value}");
                }
            }
        }
    }

    public static List<GrodItem> ReadGrif(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        string key = string.Empty;
        string value = string.Empty;
        List<GrodItem> items = [];
        var inCommentBlock = false;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("//"))
            {
                // Comment line, ignore
                continue;
            }
            if (trimmedLine.StartsWith("/*"))
            {
                inCommentBlock = true;
            }
            if (inCommentBlock)
            {
                if (trimmedLine.EndsWith("*/"))
                {
                    inCommentBlock = false;
                }
                continue;
            }
            if (!line.StartsWith('\t') && !line.StartsWith(' '))
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (value == NULL)
                    {
                        items.Add(new(key, null));
                    }
                    else
                    {
                        items.Add(new(key, value));
                    }
                }
                // New key found, reset value
                key = trimmedLine;
                value = string.Empty;
            }
            else
            {
                if (value.Length > 0)
                {
                    if (!value.EndsWith(NL, OIC) &&
                        !value.EndsWith(TAB, OIC) &&
                        !value.EndsWith(SPACE, OIC) &&
                        !trimmedLine.StartsWith(NL, OIC) &&
                        !trimmedLine.StartsWith(TAB, OIC) &&
                        !trimmedLine.StartsWith(SPACE, OIC))
                    {
                        value += ' ';
                    }
                }
                value += trimmedLine;
            }
        }
        if (!string.IsNullOrWhiteSpace(key))
        {
            if (value == NULL)
            {
                items.Add(new(key, null));
            }
            else
            {
                items.Add(new(key, value));
            }
        }
        return items;
    }

    /// <summary>
    /// Renders the output from Dags.Process into a single string.
    /// </summary>
    public static StringBuilder RenderOutput(List<DagsItem> items)
    {
        StringBuilder output = new();
        foreach (var item in items)
        {
            switch (item.Type)
            {
                case DagsType.Text:
                    output.Append(HandleText(item.Value));
                    break;
                case DagsType.Internal:
                    output.AppendLine($"INTERNAL: {HandleText(item.Value)}");
                    break;
                case DagsType.Error:
                    output.AppendLine();
                    output.AppendLine($"ERROR: {HandleText(item.Value)}");
                    break;
                default:
                    output.AppendLine($"Unknown Item Type: {item.Type}");
                    output.AppendLine($"Value: {HandleText(item.Value)}");
                    break;
            }
        }
        return output;
    }

    /// <summary>
    /// Interpret escape sequences. Handles "\s" as space and "\"" as a literal quote. Handles unicode escape sequences "\uXXXX".
    /// </summary>
    public static string HandleText(string value)
    {
        StringBuilder output = new();
        StringBuilder unicodeDigits = new();
        var lastSlash = false;
        var inUnicode = false;
        unicodeDigits.Clear();
        foreach (char c in value)
        {
            if (inUnicode)
            {
                // Expecting 4 hex digits
                if (Uri.IsHexDigit(c))
                {
                    unicodeDigits.Append(c);
                    if (unicodeDigits.Length == 4)
                    {
                        var hex = unicodeDigits.ToString();
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                        {
                            output.Append((char)codePoint);
                        }
                        else
                        {
                            // Invalid unicode sequence, treat literally
                            output.Append("\\u").Append(hex);
                        }
                        unicodeDigits.Clear();
                        inUnicode = false;
                    }
                }
                else
                {
                    // Invalid unicode sequence, treat literally
                    output.Append("\\u").Append(c);
                    unicodeDigits.Clear();
                    inUnicode = false;
                }
            }
            else if (lastSlash)
            {
                switch (c)
                {
                    case 'n':
                    case 'N':
                        output.Append('\n');
                        lastSlash = false;
                        break;
                    case 't':
                    case 'T':
                        output.Append('\t');
                        lastSlash = false;
                        break;
                    case 'r':
                    case 'R':
                        output.Append('\r');
                        lastSlash = false;
                        break;
                    case 'b':
                    case 'B':
                        output.Append('\b');
                        lastSlash = false;
                        break;
                    case 'f':
                    case 'F':
                        output.Append('\f');
                        lastSlash = false;
                        break;
                    case 's':
                    case 'S':
                        output.Append(' ');
                        lastSlash = false;
                        break;
                    case 'u':
                    case 'U':
                        inUnicode = true;
                        lastSlash = false;
                        break;
                    case '"':
                        output.Append('"');
                        lastSlash = false;
                        break;
                    case '\\':
                        output.Append('\\');
                        lastSlash = false;
                        break;
                    default:
                        output.Append('\\').Append(c);
                        lastSlash = false;
                        break;
                }
            }
            else if (c == '\\')
            {
                lastSlash = true;
            }
            else
            {
                output.Append(c);
                lastSlash = false;
            }
        }
        if (inUnicode)
        {
            // If we were in a unicode sequence but didn't complete it, append the remaining digits
            output.Append("\\u").Append(unicodeDigits);
        }
        if (lastSlash)
        {
            output.Append('\\'); // If the last character was a backslash, append it
        }
        return output.ToString();
    }
}
