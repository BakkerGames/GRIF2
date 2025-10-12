using System.Text;
using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    public static void WriteGrif(string filePath, List<GrodItem> items, bool jsonFormat)
    {
        var needsComma = false;
        string value;
        using var writer = new StreamWriter(filePath);
        writer.Write(HeaderComment(filePath, jsonFormat));
        if (jsonFormat)
        {
            writer.WriteLine("{");
        }
        foreach (var item in items)
        {
            if (jsonFormat)
            {
                if (needsComma)
                {
                    writer.WriteLine(",");
                }
                writer.Write('\t');
                writer.Write('\"');
                writer.Write(EncodeString(item.Key));
                writer.Write("\":");
            }
            else
            {
                writer.WriteLine(item.Key);
            }
            if (item.Value != null && item.Value.StartsWith('@'))
            {
                // If the value starts with '@', it is a script
                if (jsonFormat)
                {
                    writer.Write(" \"");
                    value = Dags.CompressScript(item.Value);
                    writer.Write(EncodeString(value));
                    writer.Write('\"');
                    needsComma = true;
                }
                else
                {
                    writer.WriteLine(Dags.PrettyScript(item.Value, true));
                }
            }
            else
            {
                value = item.Value ?? NULL;
                if (jsonFormat)
                {
                    writer.Write(" \"");
                    writer.Write(EncodeString(value));
                    writer.Write('\"');
                    needsComma = true;
                }
                else
                {
                    // Otherwise, write the value without quotes
                    if (value == "") value = "\"\"";
                    while (value.Length > 2 && value.Contains(NL, OIC))
                    {
                        var pos = value.IndexOf(NL, OIC);
                        writer.WriteLine($"\t{value[..(pos + 2)]}");
                        value = value[(pos + 2)..]; // Skip the escape sequence
                        if (value.StartsWith(' '))
                        {
                            value = SPACE + value[1..];
                        }
                    }
                    if (value.Length > 0)
                    {
                        if (value.StartsWith(' '))
                        {
                            value = SPACE + value[1..];
                        }
                        if (value.EndsWith(' '))
                        {
                            value = value[..^1] + SPACE;
                        }
                        writer.WriteLine($"\t{value}");
                    }
                }
            }
        }
        if (jsonFormat)
        {
            writer.WriteLine();
            writer.WriteLine("}");
        }
    }

    public static List<GrodItem> ReadGrif(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        string key = string.Empty;
        string value = string.Empty;
        List<GrodItem> items = [];
        var inCommentBlock = false;
        var jsonFormat = false;
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
            if (trimmedLine == "{" && items.Count == 0)
            {
                jsonFormat = true;
                continue;
            }
            if (jsonFormat && trimmedLine == "}")
            {
                continue;
            }
            if (jsonFormat)
            {
                if (trimmedLine.EndsWith(','))
                {
                    trimmedLine = trimmedLine[..^1];
                }
                var pos = trimmedLine.IndexOf("\": \"");
                key = trimmedLine[..pos].Trim()[1..];
                value = trimmedLine[(pos + 4)..].Trim()[..^1];
                items.Add(new(key, value));
                key = "";
                value = "";
            }
            else
            {
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
                    if (trimmedLine.Contains(SPACE))
                    {
                        trimmedLine = trimmedLine.Replace(SPACE, " ");
                    }
                    value += trimmedLine;
                }
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
        int index = 0;
        while (index < items.Count)
        {
            var item = items[index];
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
                    HandleOutChannel(grod, items, ref index);
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
            index++;
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
        var prompt = grod.Get(PROMPT, true) ?? "> ";
        var promptProcess = Dags.Process(grod, prompt);
        RenderOutput(grod, promptProcess);
    }

    public static void AfterPrompt(Grod grod)
    {
        var afterPrompt = grod.Get(AFTER_PROMPT, true) ?? "";
        var afterProcess = Dags.Process(grod, afterPrompt);
        RenderOutput(grod, afterProcess);
    }

    public static string GetInput(Grod grod)
    {
        string result = "";
        string resultRaw = "";
        while (InputIndex < InputLines.Count && string.IsNullOrWhiteSpace(result))
        {
            resultRaw = InputLines[InputIndex++].Trim();
            if (resultRaw.StartsWith("//"))
            {
                Console.Write(resultRaw + Environment.NewLine);
                OutStream?.Write(resultRaw.Replace("\n", Environment.NewLine) + Environment.NewLine);
            }
            result = resultRaw.Trim();
            if (result.Contains("//"))
            {
                result = result[..result.IndexOf("//")].Trim();
            }
        }
        if (!string.IsNullOrWhiteSpace(result))
        {
            Console.Write(resultRaw + Environment.NewLine);
        }
        while (string.IsNullOrWhiteSpace(result))
        {
            result = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(result))
            {
                Prompt(grod);
            }
        }
        OutStream?.Write(resultRaw.Replace("\n", Environment.NewLine) + Environment.NewLine);
        return result;
    }

    public static string GetSavePath(Grod grod, string filebase, string fileext)
    {
        var result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        result = Path.Combine(result, APP_NAME);
        result = Path.Combine(result, grod.Get(GAMENAME, true)!);
        if (!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        result = Path.Combine(result, filebase + fileext);
        return result;
    }

    private static string EncodeString(string value)
    {
        StringBuilder result = new();
        var isScript = value.StartsWith('@');
        foreach (char c in value)
        {
            if (c < ' ' || c > '~')
            {
                if (isScript && (c == '\r' || c == '\n' || c == '\t'))
                {
                    result.Append(c);
                }
                else if (c == '\r')
                {
                    result.Append(@"\r");
                }
                else if (c == '\n')
                {
                    result.Append(@"\n");
                }
                else if (c == '\t')
                {
                    result.Append(@"\t");
                }
                else
                {
                    result.Append(@"\u");
                    result.Append($"{(int)c:x4}");
                }
            }
            else if (c == '"' || c == '\\')
            {
                result.Append('\\');
                result.Append(c);
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }

    private static string HeaderComment(string path, bool jsonFormat)
    {
        StringBuilder result = new();
        result.Append("// ");
        result.Append(Path.GetFileName(path));
        result.Append(" - ");
        result.Append(jsonFormat ? "JSON format" : "GRIF format");
        result.AppendLine();
        return result.ToString();
    }

}
