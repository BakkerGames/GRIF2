
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
        }
        else
        {
            var p = GetParameters(tokens, ref index, grod);
            switch (token)
            {
                case "@get(":
                    CheckParameterCount(p, 1);
                    var value = grod.Get(p[0].Value, true) ?? "";
                    result.Add(new DagsItem(1, value));
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
                            case 1: // Static value, change to writable
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

    private static void CheckParameterCount(List<DagsItem> p, int count)
    {
        if (p.Count != count)
        {
            throw new ArgumentException($"Expected {count} parameters, but got {p.Count}");
        }
    }

    private static List<DagsItem> GetParameters(string[] tokens, ref int index, Grod grod)
    {
        List<DagsItem> parameters = [];
        while (index < tokens.Length && tokens[index] != ")")
        {
            var token = tokens[index++];
            if (token.StartsWith('@'))
            {
                // Handle nested tokens
                parameters.AddRange(ProcessOneCommand(tokens, ref index, grod));
            }
            else
            {
                // Add static value
                parameters.Add(new DagsItem(1, token));
            }
            if (index < tokens.Length)
            {
                if (tokens[index] == ")")
                {
                    break; // End of parameters
                }
                if (tokens[index] != ",")
                {
                    throw new SystemException("Missing comma");
                }
                index++; // Skip the comma
            }
        }
        if (index >= tokens.Length || tokens[index] != ")")
        {
            throw new SystemException("Missing closing parenthesis");
        }
        index++; // Skip the closing parenthesis
        return parameters;
    }
}
