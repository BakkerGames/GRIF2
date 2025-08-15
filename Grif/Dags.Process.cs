
namespace Grif;

public partial class Dags
{
    private static List<DagsItem> ProcessOneCommand(string[] tokens, ref int index, Grod grod)
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
                    break;
                case "@nl":
                    result.Add(new DagsItem(0, "\\n")); // Add newline
                    break;
                case "@return":
                    index = tokens.Length; // End processing
                    break;
                default:
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
                    var value = grod.Get(p[0].Value, true) ?? "";
                    if (value.StartsWith('@'))
                    {
                        // script value
                        result.Add(new DagsItem(2, value));
                    }
                    else
                    {
                        // static value
                        result.Add(new DagsItem(1, value));
                    }
                    break;
                case "@set(":
                    CheckParameterCount(p, 2);
                    grod.Set(p[0].Value, p[1].Value);
                    break;
                case "@write(":
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
                    throw new SystemException($"Unknown token: {token}");
            }
        }

        return result;
    }
}
