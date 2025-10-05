using System.Diagnostics.CodeAnalysis;
using static Grif.Common;

namespace Grif;

/*
Can handle the following input patterns:
    verb
    direction
    verb direction
    verb noun
    verb noun preposition indirectnoun
    verb preposition noun
The order of the words does not matter, so "go west" and "west go" are equivalent.
Noun must come before indirect noun if both are present.
*/

public static partial class Grif
{
    private static int _maxWordLen = 0;
    private const string VERB_PREFIX = "verb.";
    private const string NOUN_PREFIX = "noun.";
    private const string DIRECTION_PREFIX = "direction.";
    private const string PREPOSITION_PREFIX = "preposition.";
    private const string COMMAND_PREFIX = "command.";
    private static string DONT_UNDERSTAND_TEXT = "";
    private static List<GrodItem> _verbs = [];
    private static List<GrodItem> _nouns = [];
    private static List<GrodItem> _directions = [];
    private static List<GrodItem> _prepositions = [];

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
        _directions = grod.Items(DIRECTION_PREFIX, true, true)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Value != NULL)
            .Select(x => new GrodItem(x.Key[DIRECTION_PREFIX.Length..], x.Value))
            .ToList();
        _prepositions = grod.Items(PREPOSITION_PREFIX, true, true)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Value != NULL)
            .Select(x => new GrodItem(x.Key[PREPOSITION_PREFIX.Length..], x.Value))
            .ToList();
        _maxWordLen = Dags.GetIntValue(grod.Get(WORDSIZE, true));
        if (_maxWordLen > 0)
        {
            TrimSynonyms(ref _verbs);
            TrimSynonyms(ref _nouns);
            TrimSynonyms(ref _directions);
            TrimSynonyms(ref _prepositions);
        }
        DONT_UNDERSTAND_TEXT = grod.Get(DONT_UNDERSTAND, true) ??
            "I don't understand \"{0}\".\\n";
    }

    public static List<DagsItem>? ParseInput(Grod grod, string input)
    {
        var result = new List<DagsItem>();
        string? verb;
        string? verbWord;
        string? direction = null;
        string? directionWord = null;
        string? noun = null;
        string? nounWord = null;
        string? preposition = null;
        string? prepositionWord = null;
        string? indirectNoun = null;
        string? indirectNounWord = null;
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
        // handle "west", "go west", "west go"
        (verb, verbWord) = GetMatchingWord(_verbs, ref words);
        if (words.Count > 0)
        {
            (direction, directionWord) = GetMatchingWord(_directions, ref words);
        }
        if (words.Count > 0)
        {
            (noun, nounWord) = GetNoun(_nouns, ref words);
        }
        if (words.Count > 0)
        {
            (preposition, prepositionWord) = GetMatchingWord(_prepositions, ref words);
            if (words.Count > 0)
            {
                (indirectNoun, indirectNounWord) = GetNoun(_nouns, ref words);
            }
            else
            {
                result.Add(new DagsItem(DagsType.Text, string.Format(DONT_UNDERSTAND_TEXT, input)));
                return result;
            }
        }
        if (verb == null && direction == null)
        {
            result.Add(new DagsItem(DagsType.Text, string.Format(DONT_UNDERSTAND_TEXT, input)));
            return result;
        }
        if (words.Count > 0)
        {
            result.Add(new DagsItem(DagsType.Text, string.Format(DONT_UNDERSTAND_TEXT, string.Join(", ", words))));
            return result;
        }
        string command;
        if (verb != null && direction != null)
        {
            command = $"{COMMAND_PREFIX}{verb}.{direction}";
        }
        else if (direction != null)
        {
            command = $"{COMMAND_PREFIX}{direction}";
            verb = direction;
            verbWord = directionWord;
            direction = null;
            directionWord = null;
        }
        else
        {
            command = $"{COMMAND_PREFIX}{verb}";
        }
        if (noun != null)
        {
            if (preposition != null && indirectNoun != null)
            {
                var tempCommand = $"{command}.{noun}.{preposition}.{indirectNoun}";
                if (grod.Get(tempCommand, true) != null)
                {
                    command = tempCommand;
                }
                else
                {
                    tempCommand = $"{command}.{noun}.{preposition}.*"; // any indirect noun
                    if (grod.Get(tempCommand, true) != null)
                    {
                        command = tempCommand;
                    }
                    else
                    {
                        command = $"{command}.*.{preposition}.*"; // any noun, any indirect noun
                    }
                }
            }
            else if (preposition != null) // no indirect noun
            {
                var tempCommand = $"{command}.{preposition}.{noun}";
                if (grod.Get(tempCommand, true) != null)
                {
                    command = tempCommand;
                }
                else
                {
                    command = $"{command}.{preposition}.*"; // any noun
                }
            }
            else if (grod.Get($"{command}.{noun}", true) != null)
            {
                command += $".{noun}";
            }
            else
            {
                command += ".*"; // any noun
            }
        }
        if (grod.Get(command, true) == null)
        {
            result.Add(new DagsItem(DagsType.Text, string.Format(DONT_UNDERSTAND_TEXT, input)));
            return result;
        }
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.full,\"{input}\")"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.verb,{verb ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.verbword,{verbWord ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.direction,{direction ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.directionword,{directionWord ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.noun,{noun ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.nounword,{nounWord ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.preposition,{preposition ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.prepositionword,{prepositionWord ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.indirectnoun,{indirectNoun ?? NULL})"));
        result.Add(new DagsItem(DagsType.Internal, $"@set(input.indirectnounword,{indirectNounWord ?? NULL})"));
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

    private static (string?, string?) GetMatchingWord(List<GrodItem> vocabList, ref List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {
            var word = words[i];
            if (_maxWordLen > 0 && word.Length > _maxWordLen)
            {
                word = word[.._maxWordLen];
            }
            foreach (var item in vocabList)
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

    private static (string?, string?) GetNoun(List<GrodItem> nounList, ref List<string> words)
    {
        (var noun, var nounWord) = GetMatchingWord(nounList, ref words);
        if (noun != null)
        {
            return (noun, nounWord);
        }
        for (int i = 0; i < words.Count; i++)
        {
            if (int.TryParse(words[i], out int number))
            {
                words.RemoveAt(i);
                return ("#", number.ToString()); // numeric noun, normalized
            }
        }
        return (null, null);
    }
}
