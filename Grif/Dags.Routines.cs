using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    public static string[] SplitTokens(string script)
    {
        List<string> result = [];
        StringBuilder token = new();
        bool inToken = false;
        bool inQuote = false;
        bool inAtFunction = false;
        bool lastSlash = false;
        bool allowsEmpty = false;
        foreach (char c in script)
        {
            if (inQuote)
            {
                if (c == '"' && !lastSlash)
                {
                    token.Append(c);
                    if (inAtFunction)
                    {
                        result.Add(token.ToString());
                        inAtFunction = false;
                    }
                    else
                    {
                        result.Add(TrimQuotes(token.ToString()));
                    }
                    token.Clear();
                    inQuote = false;
                    inToken = false;
                    continue;
                }
                if (c == '"' && lastSlash)
                {
                    token.Append("\\\\\"");
                    lastSlash = false;
                    continue;
                }
                if (c == '\\' && !lastSlash)
                {
                    lastSlash = true;
                    continue;
                }
                if (lastSlash)
                {
                    token.Append('\\');
                    lastSlash = false;
                }
                token.Append(c);
                continue;
            }
            if (c == ',' || c == ')' || c == '[' || c == ']')
            {
                if (token.Length > 0)
                {
                    if (inAtFunction)
                    {
                        result.Add(token.ToString());
                        inAtFunction = false;
                    }
                    else
                    {
                        result.Add(token.ToString());
                    }
                    token.Clear();
                }
                result.Add(c.ToString());
                inToken = false;
                continue;
            }
            if (!inToken)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                if (c == '"')
                {
                    inQuote = true;
                    token.Append(c);
                }
                else
                {
                    inAtFunction = (c == '@');
                    token.Append(c);
                }
                inToken = true;
                continue;
            }
            if (c == '@')
            {
                result.Add(token.ToString());
                token.Clear();
                token.Append(c);
                continue;
            }
            if (c == '(')
            {
                if (token.Length > 0)
                {
                    token.Append(c);
                    if (inAtFunction)
                    {
                        result.Add(token.ToString());
                        inAtFunction = false;
                    }
                    else
                    {
                        result.Add(token.ToString());
                    }
                    token.Clear();
                }
                inToken = false;
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                if (token.Length > 0)
                {
                    if (inAtFunction)
                    {
                        result.Add(token.ToString());
                        inAtFunction = false;
                    }
                    else
                    {
                        result.Add(token.ToString());
                    }
                    token.Clear();
                }
                inToken = false;
                continue;
            }
            token.Append(c);
        }
        if (token.Length > 0)
        {
            result.Add(token.ToString());
            token.Clear();
        }
        return [.. result];
    }

    /// <summary>
    /// Format the script with line breaks and indents.
    /// Parameter "indent" adds one extra tab at the beginning of each line.
    /// </summary>
    public static string PrettyScript(string script, bool indent = false)
    {
        StringBuilder result = new();

        if (!script.TrimStart().StartsWith('@') && !script.TrimStart().StartsWith('['))
        {
            if (indent)
            {
                result.Append('\t');
            }
            result.Append(script);
            return result.ToString();
        }

        int startIndent = indent ? 1 : 0;
        int indentLevel = startIndent;
        int parens = 0;
        bool ifLine = false;
        bool forLine = false;
        bool forEachKeyLine = false;
        bool forEachListLine = false;
        bool inList = false;
        bool inArray = false;
        bool lastComma = false;
        var tokens = Dags.SplitTokens(script);

        foreach (string s in tokens)
        {
            // handle lists and arrays
            if (s == "[")
            {
                if (!inList)
                {
                    inList = true;
                    if (inArray && !lastComma)
                    {
                        result.AppendLine(",");
                    }
                }
                else
                {
                    inArray = true;
                    result.AppendLine();
                    indentLevel++;
                }
                if (indentLevel > 0)
                {
                    result.Append(new string('\t', indentLevel));
                }
                result.Append(s);
                lastComma = false;
                continue;
            }
            if (s == "]")
            {
                if (inList)
                {
                    inList = false;
                }
                else
                {
                    inArray = false;
                    if (!lastComma)
                    {
                        result.AppendLine();
                    }
                    if (indentLevel > startIndent) indentLevel--;
                    if (indentLevel > 0)
                    {
                        result.Append(new string('\t', indentLevel));
                    }
                }
                result.Append(s);
                lastComma = false;
                continue;
            }
            if (s == "," && inArray && !inList)
            {
                result.AppendLine(s);
                lastComma = true;
                continue;
            }
            if (inArray || inList)
            {
                result.Append(s);
                lastComma = false;
                continue;
            }
            // handle everything else
            switch (s.ToLower())
            {
                case "@elseif":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case "@else":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case "@endif":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case "@endfor":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case "@endforeachkey":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case "@endforeachlist":
                    if (indentLevel > startIndent) indentLevel--;
                    break;
            }
            if (parens == 0)
            {
                if (ifLine)
                {
                    result.Append(' ');
                }
                else
                {
                    if (result.Length > 0)
                    {
                        result.AppendLine();
                    }
                    if (indentLevel > 0)
                    {
                        result.Append(new string('\t', indentLevel));
                    }
                }
            }
            result.Append(s);
            switch (s.ToLower())
            {
                case "@if":
                    ifLine = true;
                    break;
                case "@elseif":
                    ifLine = true;
                    break;
                case "@else":
                    indentLevel++;
                    break;
                case "@then":
                    indentLevel++;
                    ifLine = false;
                    break;
                case "@for(":
                    forLine = true;
                    break;
                case "@foreachkey(":
                    forEachKeyLine = true;
                    break;
                case "@foreachlist(":
                    forEachListLine = true;
                    break;
            }
            if (s.EndsWith('('))
            {
                parens++;
            }
            else if (s == ")")
            {
                if (parens > 0) parens--;
                if (forLine && parens == 0)
                {
                    forLine = false;
                    indentLevel++;
                }
                else if (forEachKeyLine && parens == 0)
                {
                    forEachKeyLine = false;
                    indentLevel++;
                }
                else if (forEachListLine && parens == 0)
                {
                    forEachListLine = false;
                    indentLevel++;
                }
            }
        }
        return result.ToString();
    }

    private static List<DagsItem> GetParameters(string[] tokens, ref int index, Grod grod)
    {
        List<DagsItem> parameters = [];
        while (index < tokens.Length && tokens[index] != ")")
        {
            var token = tokens[index];
            if (token.StartsWith('@'))
            {
                // Handle nested tokens
                parameters.AddRange(ProcessOneCommand(tokens, ref index, grod));
            }
            else
            {
                parameters.Add(new DagsItem(DagsType.Internal, token));
                index++;
            }
            if (index < tokens.Length)
            {
                if (tokens[index] == ")")
                {
                    break; // End of parameters
                }
                if (tokens[index] != ",")
                {
                    throw new SystemException("Missing comma in parameters");
                }
                index++; // Skip the comma
            }
        }
        if (index >= tokens.Length || tokens[index] != ")")
        {
            throw new SystemException("Missing closing parenthesis");
        }
        index++; // Skip the closing parenthesis
        return parameters;
    }

    private static string TrimQuotes(string value)
    {
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
        {
            value = value[1..^1]; // Remove surrounding quotes
            value = value.Replace("\\\"", "\"");
        }
        return value;
    }

    private static void CheckParameterCount(List<DagsItem> p, int count)
    {
        if (p.Count != count)
        {
            throw new ArgumentException($"Expected {count} parameters, but got {p.Count}");
        }
    }

    private static void CheckParameterAtLeastOne(List<DagsItem> p)
    {
        if (p.Count == 0)
        {
            throw new ArgumentException($"Expected at least one parameter, but got {p.Count}");
        }
    }

    private static int GetIntValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0; // Default to 0 if not found
        }
        if (!int.TryParse(value, out int value1))
        {
            throw new SystemException($"Invalid integer: {value}");
        }
        return value1;
    }

    private static List<DagsItem> GetUserDefinedFunctionValues(string token, List<DagsItem> p, Grod grod)
    {
        var keys = grod.Keys(true, true)
            .Where(x => x.StartsWith(token, OIC))
            .ToList();
        if (keys.Count == 0)
        {
            throw new SystemException($"Unknown token: {token}");
        }
        if (keys.Count > 1)
        {
            throw new SystemException($"Multiple definitions found for {token}");
        }
        var key = keys.First();
        if (!key.EndsWith(')'))
        {
            throw new SystemException($"Invalid key format: {key}");
        }
        var placeholders = key[(key.IndexOf('(') + 1)..^1].Split(',');
        if (placeholders.Length != p.Count)
        {
            throw new SystemException($"Parameter count mismatch for {key}. Expected {placeholders.Length}, got {p.Count}");
        }
        var value = grod.Get(key, true)
            ?? throw new SystemException($"Key not found: {keys.First()}");
        for (int i = 0; i < placeholders.Length; i++)
        {
            var placeholder = placeholders[i].Trim();
            value = value.Replace("$" + placeholder, p[i].Value, OIC);
        }
        var userResult = Process(value, grod);
        return userResult;
    }

    private static bool IsNull(string? value)
    {
        return value == null || value.Equals(NULL, OIC);
    }

    private static string TrueFalse(bool value)
    {
        return value ? TRUE : FALSE;
    }

    private static bool IsTrue(string? value)
    {
        if (value == null)
        {
            return false; // Treat null as false
        }
        return value.ToLower() switch
        {
            TRUE or "t" or "yes" or "y" or "1" or "-1" => true,
            FALSE or "f" or "no" or "n" or "0" or NULL or "" => false,
            _ => throw new SystemException($"Non-boolean value: {value}"),
        };
    }
}
