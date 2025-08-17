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
            if (item.Value.StartsWith('@'))
            {
                // If the value starts with '@', it is a script
                writer.WriteLine(Dags.PrettyScript(item.Value, true));
            }
            else
            {
                // Otherwise, write the value without quotes
                var value = item.Value;
                while (value.Length > 2 && value.Contains("\\n", OIC))
                {
                    var pos = value.IndexOf("\\n", OIC);
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
        foreach (var line in lines)
        {
            if (!line.StartsWith('\t') && !line.StartsWith(' '))
            {
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    items.Add(new(key, value));
                }
                // New key found, reset value
                key = line.Trim();
                value = string.Empty;
            }
            else
            {
                var lineTrimmed = line.Trim();
                if (value.Length > 0)
                {
                    if (!value.EndsWith("\\n") &&
                        !value.EndsWith("\\t") &&
                        !value.EndsWith("\\s") &&
                        !lineTrimmed.StartsWith("\\n") &&
                        !lineTrimmed.StartsWith("\\t") &&
                        !lineTrimmed.StartsWith("\\s"))
                    {
                        value += ' ';
                    }
                }
                value += lineTrimmed;
            }
        }
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            items.Add(new(key, value));
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
                case 0:
                    output.Append(HandleText(item.Value));
                    break;
                case -1:
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
        if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
        {
            value = value[1..^1]; // Remove surrounding quotes
        }
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
