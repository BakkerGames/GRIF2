using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private static readonly Random _random = new();

    public static List<Message> Process(Grod grod, string? script)
    {
        List<Message> items = [new Message(MessageType.Text, script ?? "")];
        return ProcessItems(grod, items);
    }

    public static List<Message> ProcessItems(Grod grod, List<Message> items)
    {
        List<Message> result = [];
        foreach (var item in items)
        {
            if (item.Type == MessageType.Error)
            {
                result.Add(item);
                continue;
            }
            if (item.Type == MessageType.Text ||
                item.Type == MessageType.Internal ||
                item.Type == MessageType.Script)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }
                if (IsScript(item.Value))
                {
                    //TODO var script = new Script(item.Value);


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
            if (item.Type == MessageType.InChannel)
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
