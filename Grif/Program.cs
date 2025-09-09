using static Grif.Common;
using static Grif.GrifIO;
using static Grif.Parser;

namespace Grif;

internal class Program
{
    private const string _version = "2.2025.0908";

    public static string Version => _version;

    static void Main(string[] args)
    {
        string filename;
        if (args.Length == 0)
        {
            Console.WriteLine("Please specify a GRIF filename.");
            return;
        }
        filename = args[0];
        if (!File.Exists(filename))
        {
            Console.WriteLine($"File not found: {filename}");
            return;
        }
        var grodBase = new Grod("base");
        List<GrodItem> items = ReadGrif(filename);
        grodBase.AddItems(items);
        var grod = new Grod("overlay", grodBase);
        RunGame(grod);
    }

    private static void RunGame(Grod grod)
    {
        try
        {
            var gameOver = false;
            int outputWidth = 0;
            if (int.TryParse(grod.Get("system.output_width", true), out int ow))
            {
                outputWidth = ow;
            }
            var intro = grod.Get("system.intro", true);
            if (!string.IsNullOrWhiteSpace(intro))
            {
                var introOutput = Dags.Process(grod, intro);
                var textOutput = RenderOutput(introOutput, outputWidth, ref gameOver);
                Console.Write(textOutput.ToString());
            }
            while (!gameOver)
            {
                // run background scripts
                var backgroundKeys = grod.Keys(true, true)
                    .Where(x => x.StartsWith("background.", OIC));
                foreach (var bgKey in backgroundKeys)
                {
                    var bgValue = grod.Get(bgKey, true) ?? "";
                    var bgProcess = Dags.Process(grod, bgValue);
                    var bgOutput = RenderOutput(bgProcess, outputWidth, ref gameOver);
                    Console.Write(bgOutput);
                    if (gameOver) { return; }
                }
                // prompt
                var prompt = grod.Get("system.prompt", true) ?? "> ";
                var promptProcess = Dags.Process(grod, prompt);
                var promptOutput = RenderOutput(promptProcess, outputWidth, ref gameOver);
                Console.Write(promptOutput);
                // input
                var input = Console.ReadLine() ?? "";
                // after prompt
                var afterPrompt = grod.Get("system.after_prompt", true) ?? "";
                var afterProcess = Dags.Process(grod, afterPrompt);
                var afterOutput = RenderOutput(afterProcess, outputWidth, ref gameOver);
                Console.Write(afterOutput);
                // process input
                var parsed = ParseInput(input, grod);
                var output = Dags.ProcessItems(grod, parsed);
                var textOutput = RenderOutput(output, outputWidth, ref gameOver);
                Console.Write(textOutput);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
    }
}
