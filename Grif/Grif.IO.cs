using System.Text;
using static Grif.Common;

namespace Grif;

public static partial class Grif
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

    public static void RenderOutput(Grod grod, List<DagsItem> items)
    {
        foreach (var item in items)
        {
            switch (item.Type)
            {
                case DagsType.Text:
                    var tempLine = HandleText(item.Value);
                    WriteOutput(tempLine);
                    break;
                case DagsType.Internal:
                    WriteOutput($"INTERNAL: {HandleText(item.Value)}");
                    break;
                case DagsType.Error:
                    WriteOutput($"ERROR: {HandleText(item.Value)}");
                    break;
                case DagsType.OutChannel:
                    HandleOutChannel(grod, item);
                    break;
                default:
                    WriteOutput($"Unknown Item Type: {item.Type}");
                    WriteOutput($"Value: {HandleText(item.Value)}");
                    break;
            }
            if (GameOver)
            {
                break;
            }
        }
    }

    public static void WriteOutput(string value)
    {
        if (OutputWidth <= 0)
        {
            Console.Write(value);
            OutStream?.Write(value.Replace("\n", Environment.NewLine));
            return;
        }
        var tempOutput = value;
        var output = new StringBuilder();
        output.Clear();
        while (CurrOutputPos + tempOutput.Length > OutputWidth)
        {
            var pos = tempOutput.IndexOf('\n');
            if (pos >= 0 && pos <= OutputWidth)
            {
                var temp1 = tempOutput[..(pos + 1)];
                output.Append(temp1);
                CurrOutputPos = 0;
                if (pos + 1 >= tempOutput.Length)
                {
                    tempOutput = "";
                }
                else
                {
                    tempOutput = tempOutput[(pos + 1)..];
                }
                continue;
            }
            pos = tempOutput.IndexOf(' ');
            if (pos >= 0 && pos + CurrOutputPos <= OutputWidth)
            {
                pos = tempOutput.LastIndexOf(' ', OutputWidth);
                var temp2 = tempOutput[..(pos + 1)].TrimEnd();
                output.Append(temp2);
                output.Append('\n');
                CurrOutputPos = 0;
                if (pos + 1 >= tempOutput.Length)
                {
                    tempOutput = "";
                }
                else
                {
                    tempOutput = tempOutput[(pos + 1)..];
                }
                continue;
            }
            if (tempOutput.Length > OutputWidth)
            {
                output.Append(tempOutput[..OutputWidth]);
                output.Append('\n');
                CurrOutputPos = 0;
                if (OutputWidth >= tempOutput.Length)
                {
                    tempOutput = "";
                }
                else
                {
                    tempOutput = tempOutput[OutputWidth..];
                }
                continue;
            }
            output.Append(tempOutput);
            CurrOutputPos += tempOutput.Length;
            tempOutput = "";
        }
        if (tempOutput.Length > 0)
        {
            output.Append(tempOutput);
            if (tempOutput.Contains('\n'))
            {
                CurrOutputPos = tempOutput.Length - tempOutput.LastIndexOf('\n') - 1;
            }
            else
            {
                CurrOutputPos += tempOutput.Length;
            }
        }
        Console.Write(output.ToString());
        OutStream?.Write(output.ToString().Replace("\n", Environment.NewLine));
    }

    public static string HandleText(string value)
    {
        // Interpret escape sequences.
        // Handles "\s" as space and "\"" as a literal quote.
        // Handles unicode escape sequences "\uXXXX".
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

    public static void Prompt(Grod grod)
    {
        var prompt = grod.Get("system.prompt", true) ?? "> ";
        var promptProcess = Dags.Process(grod, prompt);
        RenderOutput(grod, promptProcess);
    }

    public static void AfterPrompt(Grod grod)
    {
        var afterPrompt = grod.Get("system.after_prompt", true) ?? "";
        var afterProcess = Dags.Process(grod, afterPrompt);
        RenderOutput(grod, afterProcess);
    }

    public static string GetInput(Grod grod)
    {
        string result = "";
        if (InputIndex < InputLines.Count)
        {
            result = InputLines[InputIndex++];
            while (string.IsNullOrWhiteSpace(result) || result.StartsWith("//"))
            {
                result = InputLines[InputIndex++];
            }
            Console.Write(result + Environment.NewLine);
        }
        while (string.IsNullOrWhiteSpace(result))
        {
            result = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(result))
            {
                Prompt(grod);
            }
        }
        OutStream?.Write(result.Replace("\n", Environment.NewLine) + Environment.NewLine);
        return result;
    }
}
