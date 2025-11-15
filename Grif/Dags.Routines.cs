using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    /// <summary>
    /// Split the script into tokens for processing.
    /// </summary>
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
                        result.Add(token.ToString());
                    }
                    token.Clear();
                    inQuote = false;
                    inToken = false;
                    continue;
                }
                if (c == '"' && lastSlash)
                {
                    token.Append("\\\"");
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
            if (char.IsWhiteSpace(c))
            {
                if (!inToken)
                {
                    continue;
                }
                result.Add(token.ToString());
                token.Clear();
                inToken = false;
                continue;
            }
            if (!inToken)
            {
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
                case ELSEIF_TOKEN:
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case ELSE_TOKEN:
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case ENDIF_TOKEN:
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case ENDFOR_TOKEN:
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case ENDFOREACHKEY_TOKEN:
                    if (indentLevel > startIndent) indentLevel--;
                    break;
                case ENDFOREACHLIST_TOKEN:
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
                case IF_TOKEN:
                    ifLine = true;
                    break;
                case ELSEIF_TOKEN:
                    ifLine = true;
                    break;
                case ELSE_TOKEN:
                    indentLevel++;
                    break;
                case THEN_TOKEN:
                    indentLevel++;
                    ifLine = false;
                    break;
                case FOR_TOKEN:
                    forLine = true;
                    break;
                case FOREACHKEY_TOKEN:
                    forEachKeyLine = true;
                    break;
                case FOREACHLIST_TOKEN:
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

    /// <summary>
    /// Format the script in a single line with minimal spaces.
    /// </summary>
    public static string CompressScript(string script)
    {
        if (!script.TrimStart().StartsWith('@'))
        {
            return script;
        }
        StringBuilder result = new();
        var tokens = SplitTokens(script);
        char lastChar = ',';
        bool addSpace;
        foreach (string s in tokens)
        {
            addSpace = false;
            if (s.StartsWith('@'))
            {
                if (lastChar != '(' && lastChar != ',')
                {
                    addSpace = true;
                }
            }
            else
            {
                addSpace = true;
            }
            if (addSpace)
            {
                result.Append(' ');
            }
            result.Append(s);
            lastChar = s[^1];
        }
        return result.ToString();
    }

    /// <summary>
    /// Validate the script for correct syntax.
    /// </summary>
    public static bool ValidateScript(string script)
    {
        if (!script.TrimStart().StartsWith('@'))
        {
            return true;
        }
        var tokens = SplitTokens(script);
        int parens = 0;
        int ifCount = 0;
        bool inIf = false;
        int forCount = 0;
        int forEachKeyCount = 0;
        int forEachListCount = 0;
        foreach (string s in tokens)
        {
            if (s.StartsWith('@') && s.EndsWith('('))
            {
                parens++;
            }
            else if (s == ")")
            {
                parens--;
                if (parens < 0)
                {
                    return false; // More closing parens than opening
                }
            }
            switch (s.ToLower())
            {
                case IF_TOKEN:
                    ifCount++;
                    inIf = true;
                    break;
                case AND_TOKEN:
                case OR_TOKEN:
                case NOT_TOKEN:
                    if (!inIf)
                    {
                        return false; // Logical operator outside of @if
                    }
                    break;
                case THEN_TOKEN:
                case ELSE_TOKEN:
                    inIf = false;
                    if (ifCount == 0)
                    {
                        return false; // @then, @else without matching @if
                    }
                    break;
                case ELSEIF_TOKEN:
                    inIf = true;
                    if (ifCount == 0)
                    {
                        return false; // @elseif without matching @if
                    }
                    break;
                case ENDIF_TOKEN:
                    ifCount--;
                    inIf = false;
                    if (ifCount < 0)
                    {
                        return false; // More @endif than @if
                    }
                    break;
                case FOR_TOKEN:
                    forCount++;
                    break;
                case FOREACHKEY_TOKEN:
                    forEachKeyCount++;
                    break;
                case FOREACHLIST_TOKEN:
                    forEachListCount++;
                    break;
                case ENDFOR_TOKEN:
                    forCount--;
                    if (forCount < 0)
                    {
                        return false; // More @endfor than @for
                    }
                    break;
                case ENDFOREACHKEY_TOKEN:
                    forEachKeyCount--;
                    if (forEachKeyCount < 0)
                    {
                        return false; // More @endforeachkey than @foreachkey
                    }
                    break;
                case ENDFOREACHLIST_TOKEN:
                    forEachListCount--;
                    if (forEachListCount < 0)
                    {
                        return false; // More @endforeachlist than @foreachlist
                    }
                    break;
            }
        }
        if (parens != 0)
        {
            return false; // parens not balanced
        }
        if (ifCount != 0 || inIf)
        {
            return false; // if not balanced
        }
        if (forCount != 0)
        {
            return false; // for loops not balanced
        }
        if (forEachKeyCount != 0)
        {
            return false; // foreachkey loops not balanced
        }
        if (forEachListCount != 0)
        {
            return false; // foreachlist loops not balanced
        }
        return true;
    }

    /// <summary>
    /// Get integer value from string, with error handling.
    /// </summary>
    public static int GetIntValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0; // Default to 0 if not found
        }
        if (!int.TryParse(value, out int intValue))
        {
            throw new SystemException($"Invalid integer: {value}");
        }
        return intValue;
    }

    #region Private routines

    private static List<Message> GetParameters(string[] tokens, ref int index, Grod grod)
    {
        List<Message> parameters = [];
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
                parameters.Add(new Message(MessageType.Internal, TrimQuotes(token)));
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

    private static void CheckParameterCount(List<Message> p, int count)
    {
        if (p.Count != count)
        {
            throw new ArgumentException($"Expected {count} parameters, but got {p.Count}");
        }
    }

    private static void CheckParameterAtLeastOne(List<Message> p)
    {
        if (p.Count == 0)
        {
            throw new ArgumentException($"Expected at least one parameter, but got {p.Count}");
        }
    }

    private static string GetValue(Grod grod, string? value)
    {
        if (IsNull(value))
        {
            return "";
        }
        if (!value!.StartsWith('@'))
        {
            return value;
        }
        var resultItems = Process(grod, value);
        if (resultItems.Count == 1 &&
            (resultItems[0].Type == MessageType.Text || resultItems[0].Type == MessageType.Internal))
        {
            return GetValue(grod, resultItems[0].Value);
        }
        throw new SystemException("Expected a single text result.");
    }

    private static List<Message> GetUserDefinedFunctionValues(string token, List<Message> p, Grod grod)
    {
        var keys = grod.Keys(token, true, true);
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
        var userResult = Process(grod, value);
        return userResult;
    }

    public static bool IsNull(string? value)
    {
        return value == null || value.Equals(NULL, OIC);
    }

    public static bool IsNullOrEmpty(string? value)
    {
        return value == null || value.Equals(NULL, OIC) || value == "";
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

    private static void AddListItem(Grod grod, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        value = FixListItemIn(value);
        var existing = grod.Get(key, true);
        if (string.IsNullOrEmpty(existing) || IsNull(existing))
        {
            grod.Set(key, value);
        }
        else
        {
            grod.Set(key, existing + "," + value);
        }
    }

    private static void ClearArray(Grod grod, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        var list = grod.Keys(key + ":", true, true);
        foreach (var item in list)
        {
            grod.Set(item, null);
        }
    }

    private static string FixListItemIn(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return NULL;
        if (value.Contains(','))
            value = value.Replace(",", COMMA_CHAR);
        return value;
    }

    private static string? FixListItemOut(string value)
    {
        if (value == NULL)
            return null;
        if (value.Contains(COMMA_CHAR))
            value = value.Replace(COMMA_CHAR, ",");
        return value;
    }

    private static string? GetArrayItem(Grod grod, string key, int y, int x)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (y < 0 || x < 0)
        {
            throw new SystemException($"Array indexes cannot be negative: {key}: {y},{x}");
        }
        var itemKey = $"{key}:{y}";
        return GetListItem(grod, itemKey, x);
    }

    private static void SetArrayItem(Grod grod, string key, int y, int x, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (y < 0 || x < 0)
        {
            throw new SystemException($"Array indexes cannot be negative: {key}: {y},{x}");
        }
        var itemKey = $"{key}:{y}";
        SetListItem(grod, itemKey, x, value);
    }

    private static string? GetListItem(Grod grod, string key, int x)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (x < 0)
        {
            throw new SystemException($"List indexes cannot be negative: {key}: {x}");
        }
        var list = grod.Get(key, true);
        if (string.IsNullOrWhiteSpace(list) || IsNull(list))
        {
            return null;
        }
        var items = list.Split(',');
        if (x >= items.Length)
        {
            return null;
        }
        return FixListItemOut(items[x]);
    }

    private static void SetListItem(Grod grod, string key, int x, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (x < 0)
        {
            throw new SystemException($"List indexes cannot be negative: {key}: {x}");
        }
        var list = grod.Get(key, true);
        if (string.IsNullOrWhiteSpace(list) || IsNull(list))
        {
            list = NULL;
        }
        var items = list.Split(',').ToList();
        while (x >= items.Count)
        {
            items.Add(NULL);
        }
        items[x] = FixListItemIn(value);
        grod.Set(key, string.Join(',', items));
    }

    private static void InsertAtListItem(Grod grod, string key, int x, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (x < 0)
        {
            throw new SystemException($"List indexes cannot be negative: {key}: {x}");
        }
        var list = grod.Get(key, true);
        if (string.IsNullOrWhiteSpace(list) || IsNull(list))
        {
            list = NULL;
        }
        var items = list.Split(',').ToList();
        while (x >= items.Count)
        {
            items.Add(NULL);
        }
        items.Insert(x, FixListItemIn(value));
        grod.Set(key, string.Join(',', items));
    }

    private static void RemoveAtListItem(Grod grod, string key, int x)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new SystemException("Key cannot be null or empty.");
        }
        if (x < 0)
        {
            throw new SystemException($"List indexes cannot be negative: {key}: {x}");
        }
        var list = grod.Get(key, true);
        if (string.IsNullOrWhiteSpace(list) || IsNull(list))
        {
            list = NULL;
        }
        var items = list.Split(',').ToList();
        while (x >= items.Count)
        {
            return; // Nothing to remove
        }
        items.RemoveAt(x);
        grod.Set(key, string.Join(',', items));
    }

    #endregion
}
