using static Grif.Common;

namespace Grif;

public partial class Grif
{
    private const string _version = "2.2025.0920";
    public static string Version => _version;

    /// <summary>
    /// Indicates if the game has ended.
    /// </summary>
    private static bool GameOver { get; set; } = false;

    /// <summary>
    /// Text to be used as input until exhausted.
    /// </summary>
    private static List<string> InputLines { get; set; } = [];

    /// <summary>
    /// Index into input lines.
    /// </summary>
    private static int InputIndex { get; set; } = 0;

    /// <summary>
    /// Output log file of everything printed or entered in the game.
    /// </summary>
//    private static string? OutputFilename { get; set; } = null;

    private static StreamWriter? OutStream { get; set; }

    /// <summary>
    /// Width used to wrap lines of text properly. If 0 or less, no wrapping.
    /// </summary>
    private static int OutputWidth { get; set; } = 0;

    /// <summary>
    /// Current output position on the line. Used to identify when wrapping must occur.
    /// </summary>
    private static int CurrOutputPos { get; set; } = 0;

    public static void RunGame(Grod grod, string? inFile, string? outFile)
    {
        if (!string.IsNullOrWhiteSpace(outFile))
        {
            OutStream = File.CreateText(outFile);
        }
        try
        {
            if (inFile != null)
            {
                InputLines = [.. File.ReadAllLines(inFile)];
            }
            if (int.TryParse(grod.Get("system.output_width", true), out int ow))
            {
                OutputWidth = ow;
            }
            var intro = grod.Get("system.intro", true);
            if (!string.IsNullOrWhiteSpace(intro))
            {
                var introOutput = Dags.Process(grod, intro);
                RenderOutput(grod, introOutput);
            }
            while (!GameOver)
            {
                // run background scripts
                var backgroundKeys = grod.Keys(true, true)
                    .Where(x => x.StartsWith("background.", OIC));
                foreach (var bgKey in backgroundKeys)
                {
                    var bgValue = grod.Get(bgKey, true) ?? "";
                    var bgProcess = Dags.Process(grod, bgValue);
                    RenderOutput(grod, bgProcess);
                    if (GameOver)
                    {
                        return;
                    }
                }
                // input
                Prompt(grod);
                var input = GetInput(grod);
                AfterPrompt(grod);
                // process input
                var parsed = ParseInput(grod, input);
                var output = Dags.ProcessItems(grod, parsed);
                RenderOutput(grod, output);
            }
        }
        catch (Exception ex)
        {
            WriteOutput($"\\nFatal error: {ex.Message}");
        }
        OutStream?.Close();
    }
}
