namespace Grif;

public record DagsItem(int Type, string Value);

public partial class Dags
{
    private const string _version = "2.2025.0813";

    public static string Version => _version;

    public static List<DagsItem> Process(string script, Grod grod)
    {
        try
        {
            List<DagsItem> result = [];
            var tokens = SplitTokens(script);
            int index = 0;
            do
            {
                var answer = ProcessOneCommand(tokens, ref index, grod);
                foreach (var item in answer)
                {
                    if (item.Type == -1) // Error
                    {
                        return [item];
                    }
                    result.Add(item);
                }

            } while (index < tokens.Length);
            return result;
        }
        catch (Exception ex)
        {
            return [new DagsItem(-1, ex.Message)];
        }
    }
}
