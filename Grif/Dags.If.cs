namespace Grif;

public partial class Dags
{
    private static readonly StringComparison OIC = StringComparison.OrdinalIgnoreCase;
    private const string _invalidIfSyntax = "Invalid @if syntax";
    private const string NULL = "null";

    // TODO ### needs to check for nested @if statements

    private static List<DagsItem> ProcessIf(string[] tokens, ref int index, Grod grod)
    {
        // conditions
        bool notFlag;
        string token;
        while (index < tokens.Length)
        {
            notFlag = false;
            while (index < tokens.Length &&
                tokens[index].Equals("@not", OIC))
            {
                notFlag = !notFlag;
                index++;
            }
            var cond = GetCondition(tokens, ref index, grod);
            if (index >= tokens.Length)
            {
                throw new SystemException(_invalidIfSyntax);
            }
            if (notFlag)
            {
                cond = !cond;
            }
            token = tokens[index++].ToLower();
            if (token == "@then")
            {
                if (!cond)
                {
                    SkipToElseEndif(tokens, ref index);
                    if (tokens[index].Equals("@elseif", OIC))
                    {
                        index++;
                        return ProcessIf(tokens, ref index, grod);
                    }
                    if (tokens[index].Equals("@endif", OIC))
                    {
                        index++;
                        return [];
                    }
                    // @else
                    index++;
                }
                break;
            }
            if (token == "@and")
            {
                if (!cond)
                {
                    SkipOverThen(tokens, ref index);
                    SkipToElseEndif(tokens, ref index);
                    if (tokens[index].Equals("@elseif", OIC))
                    {
                        index++;
                        return ProcessIf(tokens, ref index, grod);
                    }
                    if (tokens[index].Equals("@endif", OIC))
                    {
                        index++;
                        return [];
                    }
                    // @else
                    index++;
                    break;
                }
            }
            else if (token == "@or")
            {
                if (cond)
                {
                    SkipOverThen(tokens, ref index);
                    break;
                }
            }
            else
            {
                throw new SystemException($"Unknown token in @if: {token}");
            }
        }
        // process all commands in this section
        List<DagsItem> result = [];
        while (index < tokens.Length)
        {
            token = tokens[index].ToLower();
            if (token == "@else" || token == "@elseif" || token == "@endif")
            {
                SkipOverEndif(tokens, ref index);
                return result;
            }
            result.AddRange(ProcessOneCommand(tokens, ref index, grod));
        }
        throw new SystemException(_invalidIfSyntax);
    }

    private static void SkipOverEndif(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index++];
            if (token.Equals("@endif", OIC))
            {
                return;
            }
            if (token.Equals("@if", OIC))
            {
                SkipOverEndif(tokens, ref index);
            }
        }
        throw new SystemException("Missing @endif");
    }

    private static void SkipOverThen(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index++].ToLower();
            if (token == "@then")
            {
                return;
            }
        }
        throw new SystemException("Missing @then");
    }

    private static void SkipToElseEndif(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index].ToLower();
            if (token == "@else" || token == "@elseif" || token == "@endif")
            {
                return;
            }
            index++;
            if (token.Equals("@if", OIC))
            {
                SkipOverEndif(tokens, ref index);
            }
        }
        throw new SystemException(_invalidIfSyntax);
    }

    private static bool GetCondition(string[] tokens, ref int index, Grod grod)
    {
        int int1, int2;
        while (index < tokens.Length)
        {
            var token = tokens[index++];
            if (token.StartsWith('@'))
            {
                if (!token.EndsWith('('))
                {
                    throw new SystemException($"Unknown condition in @if: {token}");
                }
                else
                {
                    var p = GetParameters(tokens, ref index, grod);
                    switch (token.ToLower())
                    {
                        case "@true(":
                            CheckParameterCount(p, 1);
                            return IsCondition(p[0].Value);
                        case "@false(":
                            CheckParameterCount(p, 1);
                            return !IsCondition(p[0].Value);
                        case "@null(":
                            CheckParameterCount(p, 1);
                            return p[0].Value == null || p[0].Value.Equals(NULL, OIC);
                        case "@eq(":
                            CheckParameterCount(p, 2);
                            if ((p[0].Value == null || p[0].Value.Equals(NULL, OIC)) &&
                                (p[1].Value == null || p[1].Value.Equals(NULL, OIC)))
                            {
                                return true; // both are null
                            }
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return false; // one is null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 == int2;
                            }
                            return p[0].Value.Equals(p[1].Value, OIC);
                        case "@ne(":
                            CheckParameterCount(p, 2);
                            if ((p[0].Value == null || p[0].Value.Equals(NULL, OIC)) &&
                                (p[1].Value == null || p[1].Value.Equals(NULL, OIC)))
                            {
                                return false; // both are null
                            }
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return true; // one is null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 != int2;
                            }
                            return !p[0].Value.Equals(p[1].Value, OIC);
                        case "@gt(":
                            CheckParameterCount(p, 2);
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return false; // one or both are null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 > int2;
                            }
                            return string.Compare(p[0].Value, p[1].Value, OIC) > 0;
                        case "@ge(":
                            CheckParameterCount(p, 2);
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return false; // one or both are null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 >= int2;
                            }
                            return string.Compare(p[0].Value, p[1].Value, OIC) >= 0;
                        case "@lt(":
                            CheckParameterCount(p, 2);
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return false; // one or both are null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 < int2;
                            }
                            return string.Compare(p[0].Value, p[1].Value, OIC) < 0;
                        case "@le(":
                            CheckParameterCount(p, 2);
                            if (p[0].Value == null || p[0].Value.Equals(NULL, OIC) ||
                                p[1].Value == null || p[1].Value.Equals(NULL, OIC))
                            {
                                return false; // one or both are null
                            }
                            if (int.TryParse(p[0].Value, out int1) &&
                                int.TryParse(p[1].Value, out int2))
                            {
                                return int1 <= int2;
                            }
                            return string.Compare(p[0].Value, p[1].Value, OIC) <= 0;
                        default:
                            throw new SystemException($"Unknown condition in @if: {token}");
                    }
                }
            }
            else
            {
                // static value
                return IsCondition(token);
            }

            throw new SystemException("Unknown condition in @if");
        }
        throw new SystemException("Invalid condition in @if");
    }

    private static bool IsCondition(string? value)
    {
        if (value == null)
        {
            return false; // Treat null as false
        }
        return value.ToLower() switch
        {
            "true" or "t" or "yes" or "y" or "1" or "-1" => true,
            "false" or "f" or "no" or "n" or "0" or NULL or "" => false,
            _ => throw new SystemException("Invalid condition in @if"),
        };
    }
}
