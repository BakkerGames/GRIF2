namespace Grif;

public partial class Dags
{
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
                tokens[index].Equals("@not", StringComparison.OrdinalIgnoreCase))
            {
                notFlag = !notFlag;
                index++;
            }
            var cond = GetCondition(tokens, ref index, grod);
            if (index >= tokens.Length)
            {
                throw new SystemException("Invalid @if syntax");
            }
            if (notFlag)
            {
                cond = !cond;
            }
            token = tokens[index++].ToLower();
            if (token == "@then")
            {
                break;
            }
            if (token == "@and")
            {
                if (!cond)
                {
                    SkipToThen(tokens, ref index);
                    index++;
                    SkipToElseEndif(tokens, ref index);
                    if (index < tokens.Length && tokens[index].Equals("@elseif", StringComparison.CurrentCultureIgnoreCase))
                    {
                        index++;
                        ProcessIf(tokens, ref index, grod);
                        return [];
                    }
                    index++;
                    break;
                }
            }
            else if (token == "@or")
            {
                if (cond)
                {
                    SkipToThen(tokens, ref index);
                    index++;
                    break;
                }
            }
            else
            {
                throw new SystemException($"Unknown token in @if: {token}");
            }
        }
        List<DagsItem> result = [];
        while (index < tokens.Length)
        {
            token = tokens[index].ToLower();
            if (token == "@else" || token == "@elseif" || token == "@endif")
            {
                SkipToEndif(tokens, ref index);
                index++;
                break;
            }
            result.AddRange(ProcessOneCommand(tokens, ref index, grod));
        }
        return result;
    }

    private static void SkipToEndif(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index].ToLower();
            if (token == "@endif")
            {
                return;
            }
            index++;
        }
    }

    private static void SkipToThen(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index].ToLower();
            if (token == "@then")
            {
                return;
            }
            index++;
        }
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
        }
    }

    private static bool GetCondition(string[] tokens, ref int index, Grod grod)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index++];
            if (token.Equals("@true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (token.Equals("@false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            throw new SystemException("Unknown condition in @if");
        }
        throw new SystemException("Invalid condition in @if");
    }
}
