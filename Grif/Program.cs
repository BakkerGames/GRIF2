using System.Reflection;
using System.Text;

namespace Grif;

internal class Program
{
    static void Main(string[] args)
    {
        List<string> fileList = [];
        string filename;
        string? inputFilename = null;
        string? outputFilename = null;
        if (args.Length == 0)
        {
            Syntax();
            return;
        }
        int index = 0;
        while (index < args.Length)
        {
            if (args[index].StartsWith('-'))
            {
                if (index + 1 >= args.Length)
                {
                    Console.WriteLine($"Argument must have a value: {args[index]}");
                    Syntax();
                    return;
                }
                if (args[index].Equals("-i", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("--input", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    inputFilename = args[index++];
                    if (!File.Exists(inputFilename))
                    {
                        Console.WriteLine($"Input file not found: {inputFilename}");
                        Syntax();
                        return;
                    }
                }
                else if (args[index].Equals("-o", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("--output", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    outputFilename = args[index++];
                    try
                    {
                        // check if file can be created
                        var outStream = File.CreateText(outputFilename);
                        outStream.Close();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Error creating output file: {outputFilename}");
                        Syntax();
                        return;
                    }
                }
                else if (args[index].Equals("-m", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("--mod", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    var modFilename = args[index++];
                    if (File.Exists(modFilename))
                    {
                        fileList.Add(modFilename);
                    }
                    else if (File.Exists(modFilename + ".grif"))
                    {
                        fileList.Add(modFilename + ".grif");
                    }
                    else if (Directory.Exists(modFilename))
                    {
                        foreach (string file in Directory.GetFiles(modFilename, "*.grif"))
                        {
                            fileList.Add(file);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"File/directory not found: {modFilename}");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown argument: {args[index++]}");
                    Syntax();
                    return;
                }
            }
            else
            {
                filename = args[index++];
                if (File.Exists(filename))
                {
                    fileList.Add(filename);
                }
                else if (File.Exists(filename + ".grif"))
                {
                    fileList.Add(filename + ".grif");
                }
                else if (Directory.Exists(filename))
                {
                    foreach (string file in Directory.GetFiles(filename, "*.grif"))
                    {
                        fileList.Add(file);
                    }
                }
                else
                {
                    Console.WriteLine($"File/directory not found: {filename}");
                }
            }
        }
        if (fileList.Count == 0)
        {
            Syntax();
            return;
        }
        // load data
        Grod? parentGrod = null;
        Grod? currentGrod = null;
        foreach (var file in fileList)
        {
            // stack up all the files in the order specified
            currentGrod = new Grod(file, parentGrod);
            List<GrodItem> items = Grif.ReadGrif(file);
            currentGrod.AddItems(items);
            parentGrod = currentGrod;
        }
        var grod = new Grod("userdata", currentGrod);
        Grif.RunGame(grod, inputFilename, outputFilename);
    }

    public static string Syntax()
    {
        Version version = Assembly.GetEntryAssembly()?.GetName().Version ?? Version.Parse("1.0.0.0");
        StringBuilder result = new();
        result.AppendLine("GRIF - Game Runner for Interactive Fiction");
        result.AppendLine();
        result.AppendLine($"Version {version}");
        result.AppendLine();
        result.AppendLine("grif <filename.grif | directory>");
        result.AppendLine("     [-i | --input  <filename>]");
        result.AppendLine("     [-o | --output <filename>]");
        result.AppendLine("     [-m | --mod    <filename.grif | directory>]");
        result.AppendLine();
        result.AppendLine("There may be multiple -m/--mod parameters.");
        return result.ToString();
    }
}
