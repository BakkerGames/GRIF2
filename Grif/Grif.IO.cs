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
                    if (value == "")
                    {
                        value = "\"\"";
                    }
                    else
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
        List<GrodItem> items = [];
        var jsonFormat = false;
        using var reader = new StreamReader(filePath);
        var content = reader.ReadToEnd();
        int index = 0;
        string key;
        string value;
        SkipWhitespace(content, ref index);
        if (index < content.Length && content[index] == '{')
        {
            jsonFormat = true;
            index++;
        }
        while (index < content.Length)
        {
            if (jsonFormat)
            {
                SkipWhitespace(content, ref index);
                if (index < content.Length)
                {
                    if (content[index] == '}')
                    {
                        index++;
                        break;
                    }
                    if (content[index] == ',')
                    {
                        index++;
                        SkipWhitespace(content, ref index);
                    }
                }
                (key, value) = ParseJsonKeyValue(content, ref index);
            }
            else
            {
                (key, value) = ParseGrifKeyValue(content, ref index);
            }
            if (!string.IsNullOrWhiteSpace(key))
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

    #region ReadGrif Routines

    private static (string key, string value) ParseJsonKeyValue(string content, ref int index)
    {
        SkipWhitespace(content, ref index);
        string key = GetJsonString(content, ref index);
        SkipWhitespace(content, ref index);
        if (index >= content.Length || content[index] != ':')
        {
            throw new FormatException("Expected ':' after key in JSON object.");
        }
        index++; // Skip past ':'
        SkipWhitespace(content, ref index);
        string value = GetJsonString(content, ref index);
        return (key, value);
    }

    private static string GetJsonString(string content, ref int index)
    {
        if (index >= content.Length || content[index] != '\"')
        {
            throw new FormatException("Expected '\"' at the start of JSON string.");
        }
        index++; // Skip past opening quote
        StringBuilder result = new();
        bool lastSlash = false;
        while (index < content.Length)
        {
            char c = content[index++];
            if (lastSlash)
            {
                switch (c)
                {
                    case 'n':
                    case 'N':
                        result.Append('\n');
                        break;
                    case 't':
                    case 'T':
                        result.Append('\t');
                        break;
                    case 'r':
                    case 'R':
                        result.Append('\r');
                        break;
                    case 'b':
                    case 'B':
                        result.Append('\b');
                        break;
                    case 'f':
                    case 'F':
                        result.Append('\f');
                        break;
                    case 'u':
                    case 'U':
                        // Expecting 4 hex digits
                        if (index + 3 < content.Length &&
                            Uri.IsHexDigit(content[index]) &&
                            Uri.IsHexDigit(content[index + 1]) &&
                            Uri.IsHexDigit(content[index + 2]) &&
                            Uri.IsHexDigit(content[index + 3]))
                        {
                            var hex = content.Substring(index, 4);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                            {
                                result.Append((char)codePoint);
                                index += 4;
                            }
                            else
                            {
                                throw new FormatException("Invalid unicode escape sequence in JSON string.");
                            }
                        }
                        else
                        {
                            throw new FormatException("Invalid unicode escape sequence in JSON string.");
                        }
                        break;
                    case '"':
                        result.Append('\"');
                        break;
                    case '\\':
                        result.Append('\\');
                        break;
                    default:
                        result.Append('\\').Append(c);
                        break;
                }
                lastSlash = false;
            }
            else if (c == '\\')
            {
                lastSlash = true;
            }
            else if (c == '\"')
            {
                // End of string
                return result.ToString();
            }
            else
            {
                result.Append(c);
            }
        }
        throw new FormatException("Unterminated string in JSON object.");
    }

    private static (string key, string value) ParseGrifKeyValue(string content, ref int index)
    {
        var needSpace = false;
        StringBuilder key = new();
        StringBuilder value = new();
        while (index < content.Length)
        {
            if (content[index] == '\r' || content[index] == '\n')
            {
                break;
            }
            key.Append(content[index++]);
        }
        while (index < content.Length && (content[index] == '\r' || content[index] == '\n'))
        {
            index++;
        }
        while (index < content.Length && (content[index] == '\t' || content[index] == ' '))
        {
            while (index < content.Length && (content[index] == '\t' || content[index] == ' '))
            {
                index++;
            }
            if (needSpace)
            {
                value.Append(' ');
            }
            while (index < content.Length && content[index] != '\r' && content[index] != '\n')
            {
                value.Append(content[index++]);
            }
            while (index < content.Length && (content[index] == '\r' || content[index] == '\n'))
            {
                index++;
            }
            needSpace = true;
        }
        var valueTemp = value.ToString().Trim();
        // change leading and trailing "\s" to spaces
        if (valueTemp.StartsWith("\\s"))
        {
            valueTemp = ' ' + valueTemp[2..];
        }
        if (valueTemp.EndsWith("\\s"))
        {
            valueTemp = valueTemp[..^2] + ' ';
        }
        return (key.ToString(), valueTemp);
    }

    private static void SkipWhitespace(string content, ref int index)
    {
        bool found;
        do
        {
            found = false;
            while (index < content.Length && char.IsWhiteSpace(content[index]))
            {
                index++;
                found = true;
            }
            if (index + 1 < content.Length && content[index] == '/' && content[index + 1] == '/')
            {
                // Single line comment
                index += 2;
                while (index < content.Length && content[index] != '\n')
                {
                    index++;
                }
                found = true;
            }
            if (index + 1 < content.Length && content[index] == '/' && content[index + 1] == '*')
            {
                // Multi-line comment
                index += 2;
                while (index + 1 < content.Length && !(content[index] == '*' && content[index + 1] == '/'))
                {
                    index++;
                }
                if (index + 1 < content.Length)
                {
                    index += 2; // Skip past the closing */
                }
                found = true;
            }
        } while (found);
    }

    #endregion
}
