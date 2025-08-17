namespace Grif;

public record DagsItem(int Type, string Value);

public partial class Dags
{
    private const string _version = "2.2025.0817";

    public static string Version => _version;

    public static List<DagsItem> Process(string script, Grod grod)
    {
        List<DagsItem> result = [];
        try
        {
            var tokens = SplitTokens(script);
            int index = 0;
            do
            {
                var answer = ProcessOneCommand(tokens, ref index, grod);
                result.AddRange(answer);
            } while (index < tokens.Length);
        }
        catch (Exception ex)
        {
            result.Add(new DagsItem(-1, ex.Message));
        }
        return result;
    }
}
