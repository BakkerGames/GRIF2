using System.Text;
using static Grif.Common;

namespace Grif;

public partial class Dags
{
    private static List<DagsItem> ProcessOneCommand(string[] tokens, ref int index, Grod grod)
    {
        List<DagsItem> result = [];
        string? value;
        int int1, int2;
        int intAnswer;
        try
        {
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
                    case "@getinchannel":
                        result.Add(new DagsItem(DagsType.Internal, grod.Get(INCHANNEL, true) ?? ""));
                        grod.Remove(INCHANNEL, false); // Clear after reading
                        break;
                    case "@nl":
                        result.Add(new DagsItem(DagsType.Text, NL));
                        break;
                    case "@return":
                        index = tokens.Length; // End processing
                        return result;
                    default:
                        value = grod.Get(token, true);
                        if (value != null)
                        {
                            var userResult = Process(grod, value);
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
                        intAnswer = int1 + int2;
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
                        break;
                    case "@addlist(":
                        CheckParameterCount(p, 2);
                        AddListItem(grod, p[0].Value, p[1].Value);
                        break;
                    case "@addto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        intAnswer = int1 + int2;
                        grod.Set(p[0].Value, intAnswer.ToString());
                        break;
                    case "@bitwiseand(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 & int2;
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
                        break;
                    case "@bitwiseor(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 | int2;
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
                        break;
                    case "@bitwisexor(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 ^ int2;
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
                        break;
                    case "@cleararray(":
                        CheckParameterCount(p, 1);
                        ClearArray(grod, p[0].Value);
                        break;
                    case "@clearbit(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0 || int2 > 30)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 ^ (int)Math.Pow(2, int2);
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
                        break;
                    case "@clearlist(":
                        CheckParameterCount(p, 1);
                        if (string.IsNullOrWhiteSpace(p[0].Value))
                        {
                            throw new SystemException($"{token}): List name cannot be blank");
                        }
                        grod.Set(p[0].Value, null);
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
                    case "@debug(":
                        CheckParameterCount(p, 1);
                        if (IsTrue(grod.Get("system.debug", true)))
                        {
                            result.Add(new DagsItem(DagsType.Text, "### "));
                            result.Add(new DagsItem(DagsType.Text, p[0].Value));
                            result.Add(new DagsItem(DagsType.Text, NL));
                        }
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
                        result.AddRange(Process(grod, value));
                        break;
                    case "@exists(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(!IsNullOrEmpty(value))));
                        break;
                    case "@false(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(!IsTrue(p[0].Value))));
                        break;
                    case "@for(":
                        CheckParameterCount(p, 3);
                        HandleFor(p, tokens, ref index, grod, result);
                        break;
                    case "@foreachkey(":
                        if (p.Count != 2 && p.Count != 3)
                        {
                            CheckParameterCount(p, 3);
                        }
                        HandleForEachKey(p, tokens, ref index, grod, result);
                        break;
                    case "@foreachlist(":
                        CheckParameterCount(p, 2);
                        HandleForEachList(p, tokens, ref index, grod, result);
                        break;
                    case "@format(":
                        CheckParameterAtLeastOne(p);
                        value = p[0].Value;
                        for (int i = 1; i < p.Count; i++)
                        {
                            value = value.Replace($"{{{i - 1}}}", p[i].Value); // {0} = p[1]
                        }
                        result.Add(new DagsItem(DagsType.Internal, value));
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
                    case "@getarray(":
                        CheckParameterCount(p, 3);
                        if (string.IsNullOrWhiteSpace(p[0].Value))
                        {
                            throw new SystemException("Array name cannot be blank");
                        }
                        int1 = GetIntValue(p[1].Value);
                        int2 = GetIntValue(p[2].Value);
                        value = GetArrayItem(grod, p[0].Value, int1, int2);
                        result.Add(new DagsItem(DagsType.Internal, value ?? ""));
                        break;
                    case "@getlist(":
                        CheckParameterCount(p, 2);
                        if (string.IsNullOrWhiteSpace(p[0].Value))
                        {
                            throw new SystemException("List name cannot be blank");
                        }
                        int1 = GetIntValue(p[1].Value);
                        value = GetListItem(grod, p[0].Value, int1);
                        result.Add(new DagsItem(DagsType.Internal, value ?? ""));
                        break;
                    case "@getvalue(":
                        CheckParameterCount(p, 1);
                        value = GetValue(grod, grod.Get(p[0].Value, true));
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
                    case "@isscript(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        if (IsNull(value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(value!.StartsWith('@'))));
                        }
                        break;
                    case "@label(":
                        CheckParameterCount(p, 1);
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
                        value = GetValue(grod, grod.Get(p[0].Value, true));
                        result.Add(new DagsItem(DagsType.Text, value));
                        result.Add(new DagsItem(DagsType.Text, NL));
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
                    case "@neg(":
                        CheckParameterCount(p, 1);
                        int1 = GetIntValue(p[0].Value);
                        int1 = -int1;
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@negto(":
                        CheckParameterCount(p, 1);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int1 = -int1;
                        grod.Set(p[0].Value, int1.ToString());
                        break;
                    case "@null(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(IsNullOrEmpty(p[0].Value))));
                        break;
                    case "@removeatlist(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[1].Value);
                        RemoveAtListItem(grod, p[0].Value, int1);
                        break;
                    case "@script(":
                        CheckParameterCount(p, 1);
                        value = grod.Get(p[0].Value, true);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var scriptResult = Process(grod, value);
                            result.AddRange(scriptResult);
                        }
                        break;
                    case "@set(":
                        CheckParameterCount(p, 2);
                        grod.Set(p[0].Value, p[1].Value);
                        break;
                    case "@setlist(":
                        CheckParameterCount(p, 3);
                        int1 = GetIntValue(p[1].Value);
                        SetListItem(grod, p[0].Value, int1, p[2].Value);
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
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(IsTrue(p[0].Value))));
                        break;
                    case "@write(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            value = GetValue(grod, item.Value);
                            result.Add(new DagsItem(DagsType.Text, value));
                        }
                        break;
                    case "@writeline(":
                        CheckParameterAtLeastOne(p);
                        foreach (var item in p) // concatenate all parameters
                        {
                            value = GetValue(grod, item.Value);
                            result.Add(new DagsItem(DagsType.Text, value));
                        }
                        result.Add(new DagsItem(DagsType.Text, NL));
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
            result.Add(new DagsItem(DagsType.Error, error.ToString()));
            return result;
        }
    }
}
