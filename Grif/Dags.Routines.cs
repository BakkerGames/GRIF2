using System.Text;

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
                        result.Add(token.ToString().Replace("\\\"", "\""));
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
                parameters.Add(new DagsItem(1, token)); // static value
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
}
