namespace Grif;

public record DagItem(int Type, string Value);

public partial class Dags
{
    private const string _version = "2.2025.0813";

    public static string Version => _version;

    public static async Task<List<DagItem>> Process(string script, Grod grod)
    {
        var result = new List<DagItem>();

        if (string.IsNullOrWhiteSpace(script))
        {
            return result; // Return empty list if script is null or empty
        }

        await ProcessOneItem(script, grod, result);

        return result;
    }
}
