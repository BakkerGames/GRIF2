using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private const string _invalidIfSyntax = "Invalid @if syntax";

    private static List<Message> ProcessIf(string[] tokens, ref int index, Grod grod)
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
        List<Message> result = [];
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
        var answer = ProcessOneCommand(tokens, ref index, grod);
        if (answer.Count != 1
            || (answer[0].Type != MessageType.Text && answer[0].Type != MessageType.Internal))
        {
            throw new SystemException("Invalid condition in @if");
        }
        return IsTrue(answer[0].Value);
    }
}
