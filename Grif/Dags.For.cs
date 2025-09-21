using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private static void HandleFor(List<DagsItem> p, string[] tokens, ref int index, Grod grod, List<DagsItem> result)
    {
        // @for(i,<start>,<end inclusive>)=...$i...@endfor
        var iterator = "$" + p[0].Value;
        var newTokens = new List<string>();
        var level = 0;
        do
        {
            var token = tokens[index++];
            if (token.Equals("@for(", OIC))
            {
                level++;
            }
            else if (token.Equals("@endfor", OIC))
            {
                if (level <= 0)
                {
                    break;
                }
                level--;
            }
            newTokens.Add(token);
        } while (index < tokens.Length);
        var int1 = int.Parse(p[1].Value);
        var int2 = int.Parse(p[2].Value);
        for (int value = int1; value <= int2; value++)
        {
            List<string> loopTokens = [];
            foreach (var token in newTokens)
            {
                if (token.Equals(iterator, OIC))
                {
                    loopTokens.Add(value.ToString());
                }
                else
                {
                    loopTokens.Add(token);
                }
            }
            string[] loopArray = [.. loopTokens];
            var loopIndex = 0;
            do
            {
                var answer = ProcessOneCommand(loopArray, ref loopIndex, grod);
                if (answer.Count > 0)
                {
                    result.AddRange(answer);
                }
            } while (loopIndex < loopArray.Length);
        }
    }

    private static void HandleForEachKey(List<DagsItem> p, string[] tokens, ref int index, Grod grod, List<DagsItem> result)
    {
        // @foreachkey(i,prefix,[suffix])=...$i...@endforeachkey
        var newTokens = new StringBuilder();
        var level = 0;
        do
        {
            var token = tokens[index++];
            if (token.Equals("@foreachkey(", OIC))
            {
                level++;
            }
            else if (token.Equals("@endforeachkey", OIC))
            {
                if (level <= 0)
                {
                    break;
                }
                level--;
            }
            if (newTokens.Length > 0)
            {
                newTokens.Append(' ');
            }
            newTokens.Append(token);
        } while (index < tokens.Length);
        var keys = grod.Keys(true, true)
            .Where(x => x.StartsWith(p[1].Value, OIC));
        foreach (string key in keys)
        {
            var value = key[p[1].Value.Length..];
            if (p.Count > 2)
            {
                if (!value.EndsWith(p[2].Value, OIC))
                    continue;
                value = value[..^p[2].Value.Length];
            }
            var script = newTokens.ToString().Replace($"${p[0].Value}", value);
            result.AddRange(Process(grod, script));
        }
    }

    private static void HandleForEachList(List<DagsItem> p, string[] tokens, ref int index, Grod grod, List<DagsItem> result)
    {
        // @foreachlist(x,listname)=...$x...@endforeachlist
        var newTokens = new StringBuilder();
        var level = 0;
        do
        {
            var token = tokens[index++];
            if (token.Equals("@foreachlist(", OIC))
            {
                level++;
            }
            else if (token.Equals("@endforeachlist", OIC))
            {
                if (level <= 0)
                {
                    break;
                }
                level--;
            }
            if (newTokens.Length > 0)
            {
                newTokens.Append(' ');
            }
            newTokens.Append(token);
        } while (index < tokens.Length);
        // p[1] holds the name of the list
        string? list = grod.Get(p[1].Value, true);
        if (!string.IsNullOrWhiteSpace(list))
        {
            var items = list.Split(',');
            foreach (string value in items)
            {
                var value2 = FixListItemOut(value);
                if (!string.IsNullOrEmpty(value2))
                {
                    var script = newTokens.ToString().Replace($"${p[0].Value}", value2);
                    result.AddRange(Process(grod, script));
                }
            }
        }
    }
}
