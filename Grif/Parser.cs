using static Grif.Common;

namespace Grif;

public static class Parser
{
    public static List<DagsItem> ParseInput(string input, Grod grod)
    {
        var result = new List<DagsItem>();
        string? verb;
        string? verbWord;
        string? noun = null;
        string? nounWord = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return result;
        }
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return result;
        }
        var mwl = grod.Get("system.wordsize", true);
        if (mwl == null || !int.TryParse(mwl, out int maxWordLen))
        {
            maxWordLen = 0;
        }
        verbWord = words[0];
        verb = GetVerb(verbWord, maxWordLen, grod);
        if (verb == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"I don't know the verb \"{words[0]})\"."));
            return result;
        }
        if (words.Length > 1)
        {
            nounWord = words[1];
            noun = GetNoun(nounWord, maxWordLen, grod);
            if (noun == null)
            {
                result.Add(new DagsItem(DagsType.Text, $"I don't know the word \"{words[1]}\"."));
                return result;
            }
        }
        var command = $"command.{verb}";
        if (noun != null)
        {
            command += $".{noun}";
            if (grod.Get(command, true) == null)
            {
                command = $"command.{verb}.*"; // any noun
            }
        }
        if (grod.Get(command, true) == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"I don't understand \"{input}\"."));
            return result;
        }
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.full,\"{input}\")"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.verb,{verb})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.verbword,{verbWord})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.noun,{noun ?? "\"\""})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.nounword,{nounWord ?? "\"\""})"));
        result.Add(new DagsItem(DagsType.Internal, $"@script({command})"));
        return result;
    }

    private static string? GetVerb(string verb, int maxWordLen, Grod grod)
    {
        if (maxWordLen > 0 && verb.Length > maxWordLen)
        {
            verb = verb[..maxWordLen];
        }
        var verbs = grod.Keys(true, true)
            .Where(k => k.StartsWith("verb.", OIC))
            .Select(k => k[5..].ToLower())
            .ToHashSet();
        foreach (var key in verbs)
        {
            var verbWords = grod.Get($"verb.{key}", true)?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var syn in verbWords ?? [])
            {
                var synWord = syn;
                if (maxWordLen > 0 && synWord.Length > maxWordLen)
                {
                    synWord = synWord[..maxWordLen];
                }
                if (verb.Equals(synWord, OIC))
                {
                    return key.ToLower();
                }
            }
        }
        return null;
    }

    private static string? GetNoun(string noun, int maxWordLen, Grod grod)
    {
        if (maxWordLen > 0 && noun.Length > maxWordLen)
        {
            noun = noun[..maxWordLen];
        }
        var nouns = grod.Keys(true, true)
            .Where(k => k.StartsWith("noun.", OIC))
            .Select(k => k[5..].ToLower())
            .ToHashSet();
        foreach (var key in nouns)
        {
            var nounWords = grod.Get($"noun.{key}", true)?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var syn in nounWords ?? [])
            {
                var synWord = syn;
                if (maxWordLen > 0 && synWord.Length > maxWordLen)
                {
                    synWord = synWord[..maxWordLen];
                }
                if (noun.Equals(synWord, OIC))
                {
                    return key.ToLower();
                }
            }
        }
        return null;
    }
}
