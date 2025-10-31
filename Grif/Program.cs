using System.Reflection;
using System.Text;
using static Grif.Common;

namespace Grif;

internal class Program
{
    private static string? outputFilename = null;

    static void Main(string[] args)
    {
        List<string> fileList = [];
        string filename;
        string? inputFilename = null;
        if (args.Length == 0)
        {
            OutputText(Syntax());
            return;
        }
        int index = 0;
        while (index < args.Length)
        {
            if (args[index].StartsWith('-'))
            {
                if (index + 1 >= args.Length)
                {
                    OutputText($"Argument must have a value: {args[index]}");
                    OutputText(Syntax());
                    return;
                }
                if (args[index].Equals("-i", OIC) ||
                    args[index].Equals("--input", OIC))
                {
                    index++;
                    inputFilename = args[index++];
                    if (!File.Exists(inputFilename))
                    {
                        OutputText($"Input file not found: {inputFilename}");
                        OutputText(Syntax());
                        return;
                    }
                }
                else if (args[index].Equals("-o", OIC) ||
                    args[index].Equals("--output", OIC))
                {
                    index++;
                    var tempFilename = args[index++];
                    try
                    {
                        // check if file can be created
                        var outStream = File.CreateText(tempFilename);
                        outStream.Close();
                        outputFilename = tempFilename;
                    }
                    catch (Exception)
                    {
                        OutputText($"Error creating output file: {tempFilename}");
                        OutputText(Syntax());
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
                        OutputText($"File/directory not found: {modFilename}");
                    }
                }
                else
                {
                    OutputText($"Unknown argument: {args[index++]}");
                    OutputText(Syntax());
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
                    OutputText($"File/directory not found: {filename}");
                }
            }
        }
        if (fileList.Count == 0)
        {
            OutputText(Syntax());
            return;
        }
        // load data
        var game = new Game();
        game.OutputEvent += Output;
        Grod baseGrod = new(fileList[0]);
        baseGrod.AddItems(IO.ReadGrif(fileList[0]));
        for (int i = 1; i < fileList.Count; i++)
        {
            baseGrod.AddItems(IO.ReadGrif(fileList[i]));
        }
        var gameName = baseGrod.Get(GAMENAME, true);
        if (string.IsNullOrWhiteSpace(gameName))
        {
            gameName = "Unnamed Game";
        }
        game.Initialize(baseGrod, gameName, null);
        if (inputFilename != null)
        {
            try
            {
                var inStream = File.ReadAllLines(inputFilename);
                foreach (var line in inStream)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var message = new InputMessage
                        {
                            MessageType = InputMessageType.Text,
                            Content = line
                        };
                        game.InputMessages.Enqueue(message);
                    }
                }
            }
            catch (Exception)
            {
                OutputText($"Error opening input file: {inputFilename}");
                return;
            }
        }
        game.RunGame();
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

    private static void Output(object sender, OutputMessage e)
    {
        OutputText(e.Content);
    }

    private static void OutputText(string text)
    {
        Console.Write(text);
        if (outputFilename != null)
        {
            try
            {
                using var outStream = File.AppendText(outputFilename);
                outStream.Write(text);
                outStream.Flush();
            }
            catch (Exception)
            {
                // ignore file write errors
            }
        }
    }
}
