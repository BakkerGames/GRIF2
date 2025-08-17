using static Grif.Common;
using static Grif.GrifIO;

namespace Grif;

internal class Program
{
    static void Main(string[] _)
    {
        //var filename = "..\\..\\..\\..\\TestData\\test.grif";
        //var filename = "..\\..\\..\\..\\TicTacToe\\TicTacToe.grif";
        var filename = "..\\..\\..\\..\\CloakOfDarkness\\CloakOfDarkness.grif";
        var grodBase = new Grod("base");
        List<GrodItem> items = ReadGrif(filename);
        grodBase.AddItems(items);
        // TODO ### for debugging purposes
        var fileOutput = "..\\..\\..\\..\\TestData\\test_out.grif";
        WriteGrif(fileOutput, grodBase.Items(false, true));
        Console.WriteLine($"Loaded {grodBase.Count} items from {filename}");
        Console.WriteLine($"Output written to {fileOutput}");
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
                    Console.WriteLine($"{item.Key}: {item.Value.Trim()}");
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
