using static Grif.Common;

namespace Grif;

public enum DagsType
{
    Error = -1,
    Text = 0,
    Internal = 1,
    OutChannel = 2,
    InChannel = 3,
}

public record DagsItem(DagsType Type, string Value);

public partial class Dags
{
    private const string _version = "2.2025.0912";

    public static string Version => _version;

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
                if (!string.IsNullOrEmpty(item.Value)) // whitespace is allowed
                {
                    if (item.Value.StartsWith('@'))
                    {
                        var tokens = SplitTokens(item.Value);
                        int index = 0;
                        do
                        {
                            var answer = ProcessOneCommand(tokens, ref index, grod);
                            result.AddRange(answer);
                        } while (index < tokens.Length);
                    }
                    else
                    {
                        // plain text
                        result.Add(item);
                    }
                }
            }
            else if (item.Type == DagsType.InChannel)
            {
                if (!IsNull(grod.Get(INCHANNEL, true)))
                {
                    throw new Exception("DagsInChannel value is not empty.");
                }
                grod.Set(INCHANNEL, item.Value);
            }
            else
            {
                throw new Exception($"Unsupported DagsType: {item.Type}");
            }
        }
        return result;
    }
}
