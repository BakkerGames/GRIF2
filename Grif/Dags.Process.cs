using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private static List<DagsItem> ProcessOneCommand(string[] tokens, ref int index, Grod grod)
    {
        string? value;
        int value1, value2;
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
                result.Add(new DagsItem(DagsType.Intermediate, token));
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
                        result.Add(new DagsItem(DagsType.Text, "\\n")); // Add newline
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
                    case "@abs(":
                        CheckParameterCount(p, 1);
                        value1 = GetIntValue(p[0].Value);
                        if (value1 < 0)
                        {
                            value1 = -value1;
                        }
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@add(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(p[0].Value);
                        value2 = GetIntValue(p[1].Value);
                        value1 += value2;
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@addto(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(grod.Get(p[0].Value, true));
                        value2 = GetIntValue(p[1].Value);
                        value1 += value2;
                        grod.Set(p[0].Value, value1.ToString());
                        break;
                    case "@comment(":
                        // Ignore comments
                        break;
                    case "@concat(":
                        CheckParameterAtLeastOne(p);
                        StringBuilder sb = new();
                        foreach (var item in p)
                        {
                            sb.Append(item.Value);
                        }
                        result.Add(new DagsItem(DagsType.Intermediate, sb.ToString()));
                        break;
                    case "@div(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(p[0].Value);
                        value2 = GetIntValue(p[1].Value);
                        if (value2 == 0)
                        {
                            throw new SystemException("Division by zero is not allowed.");
                        }
                        value1 /= value2;
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@divto(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(grod.Get(p[0].Value, true));
                        value2 = GetIntValue(p[1].Value);
                        if (value2 == 0)
                        {
                            throw new SystemException("Division by zero is not allowed.");
                        }
                        value1 /= value2;
                        grod.Set(p[0].Value, value1.ToString());
                        break;
                    case "@exec(":
                        CheckParameterCount(p, 1);
                        value = p[0].Value;
                        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
                        {
                            value = value[1..^1]; // Remove surrounding quotes
                        }
                        result.AddRange(Process(value, grod));
                        break;
                    case "@get(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true) ?? "";
                        result.Add(new DagsItem(DagsType.Intermediate, value));
                        break;
                    case "@mod(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(p[0].Value);
                        value2 = GetIntValue(p[1].Value);
                        value1 %= value2;
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@modto(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(grod.Get(p[0].Value, true));
                        value2 = GetIntValue(p[1].Value);
                        value1 %= value2;
                        grod.Set(p[0].Value, value1.ToString());
                        break;
                    case "@msg(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        if (value == null)
                        {
                            throw new SystemException($"Message not found: {p[0].Value}");
                        }
                        result.Add(new DagsItem(DagsType.Text, value));
                        result.Add(new DagsItem(DagsType.Text, "\\n"));
                        break;
                    case "@mul(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(p[0].Value);
                        value2 = GetIntValue(p[1].Value);
                        value1 *= value2;
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@multo(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(grod.Get(p[0].Value, true));
                        value2 = GetIntValue(p[1].Value);
                        value1 *= value2;
                        grod.Set(p[0].Value, value1.ToString());
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
                    case "@setoutchannel(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.OutChannel, p[0].Value));
                        break;
                    case "@sub(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(p[0].Value);
                        value2 = GetIntValue(p[1].Value);
                        value1 -= value2;
                        result.Add(new DagsItem(DagsType.Intermediate, value1.ToString()));
                        break;
                    case "@subto(":
                        CheckParameterCount(p, 2);
                        value1 = GetIntValue(grod.Get(p[0].Value, true));
                        value2 = GetIntValue(p[1].Value);
                        value1 -= value2;
                        grod.Set(p[0].Value, value1.ToString());
                        break;
                    case "@write(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            switch (item.Type)
                            {
                                case DagsType.Intermediate:
                                    result.Add(new DagsItem(DagsType.Text, item.Value));
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
                                case DagsType.Intermediate:
                                    result.Add(new DagsItem(DagsType.Text, item.Value));
                                    break;
                                default: // return as is
                                    result.Add(item);
                                    break;
                            }
                        }
                        result.Add(new DagsItem(DagsType.Text, "\\n")); // Add newline at the end
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
