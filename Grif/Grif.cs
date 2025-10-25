using static Grif.Common;

namespace Grif;

public partial class Grif
{
    private const string _version = "2.2025.1023";

    public static string Version => _version;

    public static Action<string> StandardOutput = text => Console.Write(text);

    private static StreamWriter? OutStream { get; set; }

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
    /// Width used to wrap lines of text properly. If 0 or less, no wrapping.
    /// </summary>
    private static int OutputWidth { get; set; } = 0;

    /// <summary>
    /// Current output position on the line. Used to identify when wrapping must occur.
    /// </summary>
    private static int CurrOutputPos { get; set; } = 0;

    public static void VerifyGame(Grod grod)
    {
        if (grod.Get(GAMENAME, true) == null)
        {
            throw new Exception($"Missing value: {GAMENAME}");
        }
    }
}
