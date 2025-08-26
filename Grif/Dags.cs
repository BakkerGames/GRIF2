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
    private const string _version = "2.2025.0825";

    public static string Version => _version;

    public static List<DagsItem> Process(string script, Grod grod)
    {
        List<DagsItem> items = [new DagsItem(DagsType.Text, script)];
        return ProcessItems(items, grod);
    }

    public static List<DagsItem> ProcessItems(List<DagsItem> items, Grod grod)
    {
        List<DagsItem> result = [];
        foreach (var item in items)
        {
            if (item.Type == DagsType.Text)
            {
                var tokens = SplitTokens(item.Value);
                int index = 0;
                do
                {
                    var answer = ProcessOneCommand(tokens, ref index, grod);
                    result.AddRange(answer);
                } while (index < tokens.Length);
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
