using System.Diagnostics.CodeAnalysis;
using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    private static List<GrodItem> _verbs = [];
    private static List<GrodItem> _nouns = [];

    [SuppressMessage("Style", "IDE0305:Simplify collection initialization")]
    public static void ParseInit(Grod grod)
    {
        _verbs = grod.Items("verb.", true, true)
            .Select(x => new GrodItem(x.Key["verb.".Length..], x.Value))
            .ToList();
        _nouns = grod.Items("noun.", true, true)
            .Select(x => new GrodItem(x.Key["noun.".Length..], x.Value))
            .ToList();
    }

    public static List<DagsItem> ParseInput(Grod grod, string input)
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
        var mwl = grod.Get(WORDSIZE, true);
        if (mwl == null || !int.TryParse(mwl, out int maxWordLen))
        {
            maxWordLen = 0;
        }
        verbWord = words[0];
        verb = GetVerb(verbWord, maxWordLen);
        if (verb == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"I don't know the verb \"{words[0]}\".\\n"));
            return result;
        }
        if (words.Length > 1)
        {
            nounWord = words[1];
            noun = GetNoun(nounWord, maxWordLen);
            if (noun == null)
            {
                result.Add(new DagsItem(DagsType.Text, $"I don't know the word \"{words[1]}\".\\n"));
                return result;
            }
        }
        var command = $"command.{verb}";
        if (noun != null)
        {
            command += $".{noun}";
            if (grod.Get(command, true) == null && int.TryParse(nounWord, out int nounNumber))
            {
                command = $"command.{verb}.#"; // numeric noun
                noun = "#";
                nounWord = nounNumber.ToString();
            }
            if (grod.Get(command, true) == null)
            {
                command = $"command.{verb}.*"; // any noun
            }
        }
        if (grod.Get(command, true) == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"I don't understand \"{input}\".\\n"));
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

    private static string? GetVerb(string verb, int maxWordLen)
    {
        if (maxWordLen > 0 && verb.Length > maxWordLen)
        {
            verb = verb[..maxWordLen];
        }
        foreach (var item in _verbs)
        {
            var verbWords = item.Value?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var syn in verbWords ?? [])
            {
                var synWord = syn;
                if (maxWordLen > 0 && synWord.Length > maxWordLen)
                {
                    synWord = synWord[..maxWordLen];
                }
                if (verb.Equals(synWord, OIC))
                {
                    return item.Key.ToLower();
                }
            }
        }
        return null;
    }

    private static string? GetNoun(string noun, int maxWordLen)
    {
        if (maxWordLen > 0 && noun.Length > maxWordLen)
        {
            noun = noun[..maxWordLen];
        }
        foreach (var item in _nouns)
        {
            var nounWords = item.Value?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var syn in nounWords ?? [])
            {
                var synWord = syn;
                if (maxWordLen > 0 && synWord.Length > maxWordLen)
                {
                    synWord = synWord[..maxWordLen];
                }
                if (noun.Equals(synWord, OIC))
                {
                    return item.Key.ToLower();
                }
            }
        }
        if (int.TryParse(noun, out var _))
        {
            return "#"; // check if handled as generic number
        }
        return null;
    }
}
