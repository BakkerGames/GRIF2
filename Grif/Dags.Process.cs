using System.Text;
using static Grif.Common;

namespace Grif;

// IsNull() is equal to another IsNull(), and is less than any other value.

public partial class Dags
{
    private static List<DagsItem> ProcessOneCommand(string[] tokens, ref int index, Grod grod)
    {
        string? value;
        int int1, int2;
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
                result.Add(new DagsItem(DagsType.Internal, token));
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
                        int1 = GetIntValue(p[0].Value);
                        if (int1 < 0)
                        {
                            int1 = -int1;
                        }
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@add(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        int1 += int2;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@addto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        int1 += int2;
                        grod.Set(p[0].Value, int1.ToString());
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
                        result.Add(new DagsItem(DagsType.Internal, sb.ToString()));
                        break;
                    case "@div(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int2 == 0)
                        {
                            throw new SystemException("Division by zero is not allowed.");
                        }
                        int1 /= int2;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@divto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        if (int2 == 0)
                        {
                            throw new SystemException("Division by zero is not allowed.");
                        }
                        int1 /= int2;
                        grod.Set(p[0].Value, int1.ToString());
                        break;
                    case "@eq(":
                        CheckParameterCount(p, 2);
                        if (IsNull(p[0].Value) && IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (IsNull(p[0].Value) || IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 == int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) == 0)));
                        }
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
                    case "@false(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(!IsCondition(p[0].Value))));
                        break;
                    case "@ge(":
                        CheckParameterCount(p, 2);
                        if (IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (IsNull(p[0].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 >= int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) >= 0)));
                        }
                        break;
                    case "@get(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true) ?? "";
                        result.Add(new DagsItem(DagsType.Internal, value));
                        break;
                    case "@gt(":
                        CheckParameterCount(p, 2);
                        if (!IsNull(p[0].Value) && IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (IsNull(p[0].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 > int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) > 0)));
                        }
                        break;
                    case "@le(":
                        CheckParameterCount(p, 2);
                        if (IsNull(p[0].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 <= int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) <= 0)));
                        }
                        break;
                    case "@lt(":
                        CheckParameterCount(p, 2);
                        if (IsNull(p[0].Value) && !IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 < int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) < 0)));
                        }
                        break;
                    case "@mod(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        int1 %= int2;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@modto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        int1 %= int2;
                        grod.Set(p[0].Value, int1.ToString());
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
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        int1 *= int2;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@multo(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        int1 *= int2;
                        grod.Set(p[0].Value, int1.ToString());
                        break;
                    case "@ne(":
                        CheckParameterCount(p, 2);
                        if (IsNull(p[0].Value) && IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else if (IsNull(p[0].Value) || IsNull(p[1].Value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else if (int.TryParse(p[0].Value, out int1) &&
                            int.TryParse(p[1].Value, out int2))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(int1 != int2)));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal,
                                TrueFalse(string.Compare(p[0].Value, p[1].Value, OIC) != 0)));
                        }
                        break;
                    case "@null(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(IsNull(p[0].Value))));
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
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        int1 -= int2;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@subto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        int1 -= int2;
                        grod.Set(p[0].Value, int1.ToString());
                        break;
                    case "@true(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(IsCondition(p[0].Value))));
                        break;
                    case "@write(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            switch (item.Type)
                            {
                                case DagsType.Internal:
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
                                case DagsType.Internal:
                                    result.Add(new DagsItem(DagsType.Text, item.Value));
                                    break;
                                default: // return as is
                                    result.Add(item);
                                    break;
                            }
                        }
                        result.Add(new DagsItem(DagsType.Text, "\\n"));
                        break;
                    default:
                        var userResult = GetUserDefinedFunctionValues(token, p, grod);
                        result.AddRange(userResult);
                        break;
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
