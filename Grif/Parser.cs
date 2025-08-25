namespace Grif;

public static class Parser
{
    public static List<DagsItem> ParseInput(string input, Grod grod)
    {
        var result = new List<DagsItem>();
        if (string.IsNullOrWhiteSpace(input))
        {
            return result;
        }
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return result;
        }
        var verb = GetVerb(words[0], grod);
        if (verb == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"@set(input,\"{input}\") @script(system.unknown_input)"));
            return result;
        }
        var noun = GetNoun(words[1], grod);
        var command = verb;
        if (noun != null)
        {
            command += $".{noun}";
        }
        result.Add(new DagsItem(DagsType.Text, $"@script(command.{command})"));
        return result;
    }

    private static string? GetVerb(string verb, Grod grod)
    {
        if (grod.Get($"verb.{verb}", true) != null)
        {
            return verb.ToLower();
        }
        return null;
    }

    private static string? GetNoun(string noun, Grod grod)
    {
        if (grod.Get($"noun.{noun}", true) != null)
        {
            return noun.ToLower();
        }
        return null;
    }
}
