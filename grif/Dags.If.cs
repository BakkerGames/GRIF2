using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private const string _invalidIfSyntax = $"Invalid {IF_TOKEN} syntax";

    private static List<GrifMessage> ProcessIf(string[] tokens, ref int index, Grod grod)
    {
        // conditions
        bool notFlag;
        string token;
        while (index < tokens.Length)
        {
            notFlag = false;
            while (index < tokens.Length &&
                tokens[index].Equals(NOT_TOKEN, OIC))
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
            if (token == THEN_TOKEN)
            {
                if (!cond)
                {
                    SkipToElseEndif(tokens, ref index);
                    if (tokens[index].Equals(ELSEIF_TOKEN, OIC))
                    {
                        index++;
                        return ProcessIf(tokens, ref index, grod);
                    }
                    if (tokens[index].Equals(ENDIF_TOKEN, OIC))
                    {
                        index++;
                        return [];
                    }
                    // @else
                    index++;
                }
                break;
            }
            if (token == AND_TOKEN)
            {
                if (!cond)
                {
                    SkipOverThen(tokens, ref index);
                    SkipToElseEndif(tokens, ref index);
                    if (tokens[index].Equals(ELSEIF_TOKEN, OIC))
                    {
                        index++;
                        return ProcessIf(tokens, ref index, grod);
                    }
                    if (tokens[index].Equals(ENDIF_TOKEN, OIC))
                    {
                        index++;
                        return [];
                    }
                    // @else
                    index++;
                    break;
                }
            }
            else if (token == OR_TOKEN)
            {
                if (cond)
                {
                    SkipOverThen(tokens, ref index);
                    break;
                }
            }
            else
            {
                throw new SystemException($"Unknown token in {IF_TOKEN}: {token}");
            }
        }
        // process all commands in this section
        List<GrifMessage> result = [];
        while (index < tokens.Length)
        {
            token = tokens[index].ToLower();
            if (token == ELSE_TOKEN || token == ELSEIF_TOKEN || token == ENDIF_TOKEN)
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
            if (token.Equals(ENDIF_TOKEN, OIC))
            {
                return;
            }
            if (token.Equals(IF_TOKEN, OIC))
            {
                SkipOverEndif(tokens, ref index);
            }
        }
        throw new SystemException($"Missing {ENDIF_TOKEN}");
    }

    private static void SkipOverThen(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index++].ToLower();
            if (token == THEN_TOKEN)
            {
                return;
            }
        }
        throw new SystemException($"Missing {THEN_TOKEN}");
    }

    private static void SkipToElseEndif(string[] tokens, ref int index)
    {
        while (index < tokens.Length)
        {
            var token = tokens[index].ToLower();
            if (token == ELSE_TOKEN || token == ELSEIF_TOKEN || token == ENDIF_TOKEN)
            {
                return;
            }
            index++;
            if (token.Equals(IF_TOKEN, OIC))
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
            throw new SystemException($"Invalid condition in {IF_TOKEN}");
        }
        return IsTrue(answer[0].Value);
    }
}
