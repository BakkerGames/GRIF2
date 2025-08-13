
using System.Linq;

namespace Grif;

public partial class Dags
{
    private static async Task ProcessOneItem(string script, Grod grod, List<DagItem> result)
    {
        if (!script.StartsWith('@'))
        {
            result.Add(new DagItem(0, script));
            return;
        }

        var tokens = SplitTokens(script);

        int index = 0;
        do
        {
            index = await ProcessTokens(tokens, index, grod, result);
        } while (index < tokens.Length);
    }

    private static async Task<int> ProcessTokens(string[] tokens, int index, Grod grod, List<DagItem> result)
    {
        if (index >= tokens.Length)
        {
            return index; // Return if index is out of bounds
        }

        var token = tokens[index++];

        // static value
        if (!token.StartsWith('@'))
        {
            result.Add(new DagItem(0, token));
            return index;
        }

        // tokens without parameters
        if (!token.EndsWith('('))
        {
        }
        else
        {
            switch (token)
            {
                case "@write(":
                    result.Add(new DagItem(0, tokens[index++]));
                    index++; // Skip the closing parenthesis
                    break;
                default:
                    result.Add(new DagItem(-1, $"Unknown token: {token}"));
                    break;
            }
        }

        await Task.CompletedTask;

        return index;
    }
}
