using static Grif.Common;

namespace Grif;

public partial class Dags
{
    public static string Version { get { return "2.2025.1103"; } }

    private static readonly Random _random = new();

    public static List<DagsItem> Process(Grod grod, string? script)
    {
        List<DagsItem> items = [new DagsItem(DagsType.Text, script ?? "")];
        return ProcessItems(grod, items);
    }

    public static List<DagsItem> ProcessItems(Grod grod, List<DagsItem> items)
    {
        List<DagsItem> result = [];
        foreach (var item in items)
        {
            if (item.Type == DagsType.Error)
            {
                result.Add(item);
                continue;
            }
            if (item.Type == DagsType.Text || item.Type == DagsType.Internal)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }
                if (item.Value.StartsWith('@'))
                {
                    var tokens = SplitTokens(item.Value);
                    int index = 0;
                    do
                    {
                        var answer = ProcessOneCommand(tokens, ref index, grod);
                        if (answer.Count > 0)
                        {
                            result.AddRange(answer);
                        }
                    } while (index < tokens.Length);
                    continue;
                }
                // plain text
                result.Add(item);
                continue;
            }
            if (item.Type == DagsType.InChannel)
            {
                if (!IsNull(grod.Get(INCHANNEL, true)))
                {
                    throw new Exception("DagsInChannel value is not empty.");
                }
                grod.Set(INCHANNEL, item.Value);
                continue;
            }
            throw new Exception($"Unsupported DagsType: {item.Type}");
        }
        return result;
    }
}
