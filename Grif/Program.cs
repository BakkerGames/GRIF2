using System.Reflection;
using System.Text;
using static Grif.Common;

namespace Grif;

internal class Program
{
    static void Main(string[] args)
    {
        List<string> fileList = [];
        string filename;
        string? inputFilename = null;
        string? outputFilename = null;
        var game = new Game();
        if (args.Length == 0)
        {
            game.Output(Syntax());
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
                if (args[index].Equals("-i", OIC) ||
                    args[index].Equals("--input", OIC))
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
                else if (args[index].Equals("-o", OIC) ||
                    args[index].Equals("--output", OIC))
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
                else if (args[index].Equals("-m", OIC) ||
                    args[index].Equals("--mod", OIC))
                {
                    index++;
                    var modFilename = args[index++];
                    if (File.Exists(modFilename))
                    {
                        fileList.Add(modFilename);
                    }
                    else if (File.Exists(modFilename + DATA_EXTENSION))
                    {
                        fileList.Add(modFilename + DATA_EXTENSION);
                    }
                    else if (Directory.Exists(modFilename))
                    {
                        foreach (string file in Directory.GetFiles(modFilename, "*" + DATA_EXTENSION))
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
                else if (File.Exists(filename + DATA_EXTENSION))
                {
                    fileList.Add(filename + DATA_EXTENSION);
                }
                else if (Directory.Exists(filename))
                {
                    foreach (string file in Directory.GetFiles(filename, "*" + DATA_EXTENSION))
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
        game.Initialize(fileList[0]);
        for (int i = 1; i < fileList.Count; i++)
        {
            game.AddModule(fileList[i]);
        }
        game.RunGame(inputFilename ?? "inputFilename", outputFilename ?? "outputFilename");
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
