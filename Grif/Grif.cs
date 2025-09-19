using static Grif.Common;

namespace Grif;

public partial class Grif
{
    private const string _version = "2.2025.0918";

    public static string Version => _version;

    public static void RunGame(Grod grod)
    {
        var gameOver = false;
        int outputWidth = 0;
        int currPos = 0;
        try
        {
            if (int.TryParse(grod.Get("system.output_width", true), out int ow))
            {
                outputWidth = ow;
            }
            var intro = grod.Get("system.intro", true);
            if (!string.IsNullOrWhiteSpace(intro))
            {
                var introOutput = Dags.Process(grod, intro);
                RenderOutput(grod, introOutput, outputWidth, ref currPos, ref gameOver);
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
                    RenderOutput(grod, bgProcess, outputWidth, ref currPos, ref gameOver);
                    if (gameOver) { return; }
                }
                // input
                Prompt(grod, outputWidth, ref currPos, ref gameOver);
                var input = Console.ReadLine() ?? "";
                AfterPrompt(grod, outputWidth, ref currPos, ref gameOver);
                // process input
                var parsed = ParseInput(grod, input);
                var output = Dags.ProcessItems(grod, parsed);
                RenderOutput(grod, output, outputWidth, ref currPos, ref gameOver);
            }
        }
        catch (Exception ex)
        {
            WriteOutput($"\\nFatal error: {ex.Message}", outputWidth, ref currPos);
        }
    }
}
