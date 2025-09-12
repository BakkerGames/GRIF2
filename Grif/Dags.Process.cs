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
        bool boolAnswer;
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
                        try
                        {
                            boolAnswer = IsTrue(p[0].Value);
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(!boolAnswer)));
                        }
                        catch (Exception)
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(false)));
                        }
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
                    case "@getbit(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0 || int2 > 30)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 & (int)Math.Pow(2, int2);
                        if (intAnswer != 0)
                        {
                            intAnswer = 1;
                        }
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
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
                    case "@golabel(":
                        CheckParameterCount(p, 1);
                        for (int i = 0; i < tokens.Length - 1; i++)
                        {
                            if (tokens[i] == "@label(" && tokens[i + 1] == p[0].Value && tokens[i + 2] == ")")
                            {
                                index = i + 3;
                            }
                        }
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
                    case "@insertatlist(":
                        CheckParameterCount(p, 3);
                        int1 = GetIntValue(p[1].Value);
                        InsertAtListItem(grod, p[0].Value, int1, p[2].Value);
                        break;
                    case "@isbool(":
                        CheckParameterCount(p, 1);
                        try
                        {
                            boolAnswer = IsTrue(p[0].Value);
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        catch (Exception)
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
                        }
                        break;
                    case "@isnumber(":
                        CheckParameterCount(p, 1);
                        if (int.TryParse(p[0].Value, out _))
                        {
                            result.Add(new DagsItem(DagsType.Internal, TRUE));
                        }
                        else
                        {
                            result.Add(new DagsItem(DagsType.Internal, FALSE));
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
                    case "@listlength(":
                        CheckParameterCount(p, 1);
                        if (string.IsNullOrWhiteSpace(p[0].Value))
                        {
                            throw new SystemException($"{token}): List name cannot be blank");
                        }
                        value = grod.Get(p[0].Value, true);
                        if (IsNullOrEmpty(value))
                        {
                            result.Add(new DagsItem(DagsType.Internal, "0"));
                        }
                        else
                        {
                            var listItems = value!.Split(',');
                            result.Add(new DagsItem(DagsType.Internal, listItems.Length.ToString()));
                        }
                        break;
                    case "@lower(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, p[0].Value.ToLower()));
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
                        var tempResult = Dags.Process(grod, grod.Get(p[0].Value, true));
                        foreach (var msgItem in tempResult)
                        {
                            if (msgItem.Type == DagsType.Text || msgItem.Type == DagsType.Internal)
                            {
                                value = msgItem.Value;
                                if (!string.IsNullOrEmpty(value)) // whitespace is allowed
                                {
                                    result.Add(new DagsItem(DagsType.Text, value));
                                }
                            }
                        }
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
                    case "@rand(":
                        CheckParameterCount(p, 1);
                        int1 = GetIntValue(p[0].Value);
                        int2 = _random.Next(100);
                        boolAnswer = int2 < int1;
                        result.Add(new DagsItem(DagsType.Internal, TrueFalse(boolAnswer)));
                        break;
                    case "@removeatlist(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[1].Value);
                        RemoveAtListItem(grod, p[0].Value, int1);
                        break;
                    case "@replace(":
                        CheckParameterCount(p, 3);
                        result.Add(new DagsItem(DagsType.Internal, p[0].Value.Replace(p[1].Value, p[2].Value, OIC)));
                        break;
                    case "@rnd(":
                        CheckParameterCount(p, 1);
                        int1 = GetIntValue(p[0].Value);
                        result.Add(new DagsItem(DagsType.Internal, _random.Next(int1).ToString()));
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
                    case "@setarray(":
                        CheckParameterCount(p, 4);
                        if (string.IsNullOrWhiteSpace(p[0].Value))
                        {
                            throw new SystemException("Array name cannot be blank");
                        }
                        int1 = GetIntValue(p[1].Value); // y
                        int2 = GetIntValue(p[2].Value); // x
                        SetArrayItem(grod, p[0].Value, int1, int2, p[3].Value);
                        break;
                    case "@setbit(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(p[0].Value);
                        int2 = GetIntValue(p[1].Value);
                        if (int1 < 0 || int2 < 0 || int2 > 30)
                        {
                            throw new SystemException($"{token}{int1},{int2}): Invalid parameters");
                        }
                        intAnswer = int1 | (int)Math.Pow(2, int2);
                        result.Add(new DagsItem(DagsType.Internal, intAnswer.ToString()));
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
                    case "@substring(":
                        CheckParameterCount(p, 3);
                        int1 = GetIntValue(p[1].Value);
                        int2 = GetIntValue(p[2].Value);
                        if (int1 < 0 || int2 < 0 || int1 + int2 > p[0].Value.Length)
                        {
                            throw new SystemException($"{token}{p[0].Value},{int1},{int2}): Invalid parameters");
                        }
                        result.Add(new DagsItem(DagsType.Internal, p[0].Value.Substring(int1, int2)));
                        break;
                    case "@subto(":
                        CheckParameterCount(p, 2);
                        int1 = GetIntValue(grod.Get(p[0].Value, true));
                        int2 = GetIntValue(p[1].Value);
                        int1 -= int2;
                        grod.Set(p[0].Value, int1.ToString());
                        break;
                    case "@swap(":
                        CheckParameterCount(p, 2);
                        value = grod.Get(p[0].Value, true);
                        grod.Set(p[0].Value, grod.Get(p[1].Value, true));
                        grod.Set(p[1].Value, value);
                        break;
                    case "@tobinary(":
                        CheckParameterCount(p, 1);
                        int1 = GetIntValue(p[0].Value);
                        result.Add(new DagsItem(DagsType.Internal, Convert.ToString(int1, 2)));
                        break;
                    case "@tointeger(":
                        CheckParameterCount(p, 1);
                        int1 = Convert.ToInt32(p[0].Value, 2);
                        result.Add(new DagsItem(DagsType.Internal, int1.ToString()));
                        break;
                    case "@trim(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, p[0].Value.Trim()));
                        break;
                    case "@true(":
                        CheckParameterCount(p, 1);
                        try
                        {
                            boolAnswer = IsTrue(p[0].Value);
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(boolAnswer)));
                        }
                        catch (Exception)
                        {
                            result.Add(new DagsItem(DagsType.Internal, TrueFalse(false)));
                        }
                        break;
                    case "@upper(":
                        CheckParameterCount(p, 1);
                        result.Add(new DagsItem(DagsType.Internal, p[0].Value.ToUpper()));
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
