using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private static List<DagsItem> ProcessOneCommand(string[] tokens, ref int index, Grod grod)
    {
        string? value;
        try
        {
            List<DagsItem> result = [];
            if (index >= tokens.Length)
            {
                return result;
            }
            var token = tokens[index++];
            // static value
            if (!token.StartsWith('@'))
            {
                result.Add(new DagsItem(1, token));
                return result;
            }
            // tokens without parameters
            if (!token.EndsWith('('))
            {
                switch (token.ToLower())
                {
                    case "@if":
                        result.AddRange(ProcessIf(tokens, ref index, grod));
                        break;
                    case "@help":
                        // TODO: Implement help command
                        break;
                    case "@nl":
                        result.Add(new DagsItem(0, "\\n")); // Add newline
                        break;
                    case "@return":
                        index = tokens.Length; // End processing
                        break;
                    default:
                        value = grod.Get(token, true);
                        if (value != null)
                        {
                            var userResult = Process(value, grod);
                            result.AddRange(userResult);
                            break;
                        }
                        throw new SystemException($"Unknown token: {token}");
                }
            }
            else
            {
                var p = GetParameters(tokens, ref index, grod);
                switch (token.ToLower())
                {
                    case "@get(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true) ?? "";
                        result.Add(new DagsItem(1, value));
                        break;
                    case "@msg(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        if (value == null)
                        {
                            throw new SystemException($"Message not found: {p[0].Value}");
                        }
                        result.Add(new DagsItem(0, value));
                        result.Add(new DagsItem(0, "\\n")); // Add newline at the end
                        break;
                    case "@script(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        if (value == null)
                        {
                            throw new SystemException($"Script not found: {p[0].Value}");
                        }
                        var scriptResult = Process(value, grod);
                        result.AddRange(scriptResult);
                        break;
                    case "@set(":
                        CheckParameterCount(p, 2);
                        grod.Set(p[0].Value, p[1].Value);
                        break;
                    case "@write(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            switch (item.Type)
                            {
                                case 1: // static value, change to writable
                                    result.Add(new DagsItem(0, item.Value));
                                    break;
                                default: // return as is
                                    result.Add(item);
                                    break;
                            }
                        }
                        break;
                    case "@writeline(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            switch (item.Type)
                            {
                                case 1: // Static value, change to writable
                                    result.Add(new DagsItem(0, item.Value));
                                    break;
                                default: // return as is
                                    result.Add(item);
                                    break;
                            }
                        }
                        result.Add(new DagsItem(0, "\\n")); // Add newline at the end
                        break;
                    default:
                        var keys = grod.Keys(true, true)
                            .Where(x => x.StartsWith(token, OIC))
                            .ToList();
                        if (keys.Count > 1)
                        {
                            throw new SystemException($"Multiple definitions found for {token}");
                        }
                        if (keys.Count == 1)
                        {
                            value = grod.Get(keys.First(), true);
                            if (value == null)
                            {
                                throw new SystemException($"Key not found: {keys.First()}");
                            }
                            var userResult = Process(value, grod);
                            result.AddRange(userResult);
                            break;
                        }
                        throw new SystemException($"Unknown token: {token}");
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions and return error item
            StringBuilder error = new();
            error.AppendLine($"Error processing command at index {index}:");
            error.AppendLine(ex.Message);
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.Length > 50)
                {
                    token = string.Concat(token.AsSpan(0, 50), "...");
                }
                error.AppendLine($"{i}: {token}");
            }
            throw new SystemException(error.ToString());
        }
    }
}
