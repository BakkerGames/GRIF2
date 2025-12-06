using System.Text;
using static Grif.Common;
using static Grif.Dags;

namespace Grif;

public static class IO
{
    public static string GetSavePath(string filebase)
    {
        var result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        result = Path.Combine(result, APP_NAME);
        result = Path.Combine(result, filebase);
        if (!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        return result;
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
            if (IsScript(item.Value))
            {
                // If the value starts with '@', it is a script
                if (jsonFormat)
                {
                    writer.Write(" \"");
                    value = CompressScript(item.Value);
                    writer.Write(EncodeString(value));
                    writer.Write('\"');
                    needsComma = true;
                }
                else
                {
                    writer.WriteLine(PrettyScript(item.Value, true));
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
                            value = SPACE_CHAR + value[1..];
                        }
                        if (value.EndsWith(' '))
                        {
                            value = value[..^1] + SPACE_CHAR;
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

    #region Private

    private static string EncodeString(string value)
    {
        StringBuilder result = new();
        var isScript = IsScript(value);
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
                            if (long.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out long codePoint))
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

    #endregion
}
