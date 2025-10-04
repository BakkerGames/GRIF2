using System.Diagnostics.CodeAnalysis;
using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    private const string VERB_PREFIX = "verb.";
    private const string NOUN_PREFIX = "noun.";
    private static List<GrodItem> _verbs = [];
    private static List<GrodItem> _nouns = [];
    private static int _maxWordLen = 0;

    [SuppressMessage("Style", "IDE0305:Simplify collection initialization")]
    public static void ParseInit(Grod grod)
    {
        _verbs = grod.Items(VERB_PREFIX, true, true)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Value != NULL)
            .Select(x => new GrodItem(x.Key[VERB_PREFIX.Length..], x.Value))
            .ToList();
        _nouns = grod.Items(NOUN_PREFIX, true, true)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Value != NULL)
            .Select(x => new GrodItem(x.Key[NOUN_PREFIX.Length..], x.Value))
            .ToList();
        _maxWordLen = Dags.GetIntValue(grod.Get(WORDSIZE, true));
        if (_maxWordLen > 0)
        {
            TrimSynonyms(ref _verbs);
            TrimSynonyms(ref _nouns);
        }
    }

    public static List<DagsItem>? ParseInput(Grod grod, string input)
    {
        var result = new List<DagsItem>();
        string? verb;
        string? verbWord;
        string? noun = null;
        string? nounWord = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        if (words.Count == 0)
        {
            return null;
        }
        (verb, verbWord) = GetVerb(ref words);
        if (verb == null)
        {
            result.Add(new DagsItem(DagsType.Text, $"I don't understand \"{input}\".\\n"));
            return result;
        }
        if (words.Count > 0)
        {
            (noun, nounWord) = GetNoun(ref words);
            if (noun == null)
            {
                result.Add(new DagsItem(DagsType.Text, $"I don't understand \"{input}\".\\n"));
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

    private static void TrimSynonyms(ref List<GrodItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var words = items[i].Value!.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < words.Length; j++)
            {
                if (_maxWordLen > 0 && words[j].Length > _maxWordLen)
                {
                    words[j] = words[j][.._maxWordLen];
                }
            }
            items[i] = new GrodItem(items[i].Key, string.Join(',', words));
        }
    }

    private static (string?, string?) GetVerb(ref List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {
            var word = words[i];
            if (_maxWordLen > 0 && word.Length > _maxWordLen)
            {
                word = word[.._maxWordLen];
            }
            foreach (var item in _verbs)
            {
                var verbWords = item.Value!.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var syn in verbWords)
                {
                    if (word.Equals(syn, OIC))
                    {
                        word = words[i]; // use original string
                        words.RemoveAt(i);
                        return (item.Key, word);
                    }
                }
            }
        }
        return (null, null);
    }

    private static (string?, string?) GetNoun(ref List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {
            var word = words[i];
            if (_maxWordLen > 0 && word.Length > _maxWordLen)
            {
                word = word[.._maxWordLen];
            }
            foreach (var item in _nouns)
            {
                var nounWords = item.Value!.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var syn in nounWords)
                {
                    if (word.Equals(syn, OIC))
                    {
                        word = words[i]; // use original string
                        words.RemoveAt(i);
                        return (item.Key, word);
                    }
                }
            }
        }
        for (int i = 0; i < words.Count; i++)
        {
            if (int.TryParse(words[i], out int number))
            {
                var word = words[i];
                words.RemoveAt(i);
                return ("#", word); // numeric noun
            }
        }
        return (null, null);
    }
}
