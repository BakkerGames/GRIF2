using static Grif.Common;
using static Grif.GrifIO;
using static Grif.Parser;

namespace Grif;

internal class Program
{
    private const string _version = "2.2025.0824";

    public static string Version => _version;

    static void Main(string[] args)
    {
        Console.WriteLine($"GRIF version {Version}");
        Console.WriteLine($"DAGS version {Dags.Version}");
        Console.WriteLine($"GROD version {Grod.Version}");

        string filename;
        if (args.Length == 0)
        {
            Console.WriteLine("Please specify a GRIF filename.");
            return;
        }
        else
        {
            filename = args[0];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return;
            }
        }

        var grodBase = new Grod("base");
        List<GrodItem> items = ReadGrif(filename);
        grodBase.AddItems(items);
        Console.WriteLine($"Loaded {grodBase.Count} items from {filename}");

        //DebugGameFile(grodBase);

        RunGame(grodBase);
    }

    private static void RunGame(Grod grod)
    {
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine() ?? "";
            if (input.Equals("exit", OIC) || input.Equals("quit", OIC))
            {
                break;
            }
            var parsed = ParseInput(input, grod);
            List<DagsItem> output = [];
            try
            {
                output = Dags.ProcessItems(parsed, grod);
            }
            catch (Exception ex)
            {
                output.Add(new DagsItem(DagsType.Error, ex.Message));
            }
            if (output.Count > 0)
            {
                var textOutput = RenderOutput(output);
                Console.WriteLine(textOutput.ToString());
            }
        }
    }

    private static void DebugGameFile(Grod grodBase)
    {
        var fileOutput = "TestData\\test_out.grif";
        WriteGrif(fileOutput, grodBase.Items(false, true));
        Console.WriteLine($"Output written to {fileOutput}");
        Console.WriteLine();

        Console.WriteLine("Type 'exit' to quit, 'list' to list items, or any other command to process it.");
        do
        {
            Console.Write("> ");
            var input = Console.ReadLine() ?? "";
            if (input.Equals("exit", OIC) || input.Equals("quit", OIC))
            {
                break;
            }
            else if (input.Equals("list", OIC))
            {
                foreach (var item in grodBase.Items(false, true))
                {
                    Console.WriteLine($"{item.Key}: {item.Value?.Trim()}");
                }
            }
            else
            {
                var output = Dags.Process(input, grodBase);
                if (output.Count > 0)
                {
                    var textOutput = RenderOutput(output);
                    Console.WriteLine(textOutput.ToString());
                }
            }
        } while (true);
    }
}
