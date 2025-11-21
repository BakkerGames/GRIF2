namespace Grif;

internal class Script(string code)
{
    public string Code { get; init; } = code;

    public string[] Tokens { get; init; } = Dags.SplitTokens(code);

    public int Index { get; set; } = 0;

    public bool IsReturn { get; set; } = false;

    public string? NextToken()
    {
        if (Index >= Tokens.Length)
        {
            return null;
        }
        return Tokens[Index++];
    }
}
