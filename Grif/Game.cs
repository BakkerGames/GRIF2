using static Grif.Common;

namespace Grif;

public class Game
{
    private Grod _baseGrod = new("");
    private Grod _overlayGrod = new("");
    private readonly List<Grod> _modules = [];

    public void Initialize(string baseFilename)
    {
        var loadedItems = IO.ReadGrif(baseFilename);
        _baseGrod.Name = baseFilename;
        _baseGrod.Clear(false);
        _baseGrod.AddItems(loadedItems);
        var savePath = IO.GetSavePath(Path.GetFileNameWithoutExtension(baseFilename));
        _overlayGrod.Name = Path.Combine(savePath, SAVE_FILENAME + SAVE_EXTENSION);
        _overlayGrod.Clear(false);
        _overlayGrod.Parent = _baseGrod;
    }

    public void Initialize(string baseFilename, string overlayFilename)
    {
        var loadedBaseItems = IO.ReadGrif(baseFilename);
        var loadedOverlayItems = IO.ReadGrif(overlayFilename);
        _baseGrod.Name = baseFilename;
        _baseGrod.Clear(false);
        _baseGrod.AddItems(loadedBaseItems);
        _overlayGrod.Name = overlayFilename;
        _overlayGrod.Clear(false);
        _overlayGrod.AddItems(loadedOverlayItems);
        _overlayGrod.Parent = _baseGrod;
    }

    public void Initialize(Grod grodBase)
    {
        _baseGrod = grodBase;
        _overlayGrod.Clear(false);
        _overlayGrod.Parent = _baseGrod;
        var savePath = IO.GetSavePath(Path.GetFileNameWithoutExtension(_baseGrod.Name));
        _overlayGrod.Name = Path.Combine(savePath, SAVE_FILENAME + SAVE_EXTENSION);
    }

    public void Initialize(Grod grodBase, Grod grodOverlay)
    {
        _baseGrod = grodBase;
        _overlayGrod = grodOverlay;
        _overlayGrod.Parent = _baseGrod;
    }

    public void AddModule(string modFilename)
    {
        var loadedItems = IO.ReadGrif(modFilename);
        Grod grodMod = new(modFilename);
        grodMod.AddItems(loadedItems);
        AddModule(grodMod);
    }

    public void AddModule(Grod grodMod)
    {
        _modules.Add(grodMod);
        grodMod.Parent = _baseGrod;
        _overlayGrod.Parent = grodMod;
    }

    public void Input(string input)
    {
    }

    // Declare the delegate (if using non-generic pattern).
    public delegate void OutputEventHandler(object sender, OutputEventArgs e);

    // Declare the event.
    public event OutputEventHandler? OutputEvent;

    //public static void RunGame(string? inFile, string? outFile)
    //{
    //    ParseInit(grod);
    //    var backgroundKeys = grod.Keys("background.", true, true);
    //    if (!string.IsNullOrWhiteSpace(outFile))
    //    {
    //        OutStream = File.CreateText(outFile);
    //    }
    //    try
    //    {
    //        if (inFile != null)
    //        {
    //            InputLines = [.. File.ReadAllLines(inFile)];
    //        }
    //        if (int.TryParse(grod.Get(OUTPUT_WIDTH, true), out int ow))
    //        {
    //            OutputWidth = ow;
    //        }
    //        var intro = grod.Get(INTRO, true);
    //        if (!string.IsNullOrWhiteSpace(intro))
    //        {
    //            var introOutput = Dags.Process(grod, intro);
    //            RenderOutput(grod, introOutput);
    //        }
    //        while (!GameOver)
    //        {
    //            // run background scripts
    //            foreach (var bgKey in backgroundKeys)
    //            {
    //                var bgValue = grod.Get(bgKey, true) ?? "";
    //                var bgProcess = Dags.Process(grod, bgValue);
    //                RenderOutput(grod, bgProcess);
    //                if (GameOver)
    //                {
    //                    return;
    //                }
    //            }
    //            // input
    //            Prompt(grod);
    //            var input = GetInput(grod);
    //            AfterPrompt(grod);
    //            // process input
    //            var parsed = ParseInput(grod, input);
    //            if (parsed != null && parsed.Count > 0)
    //            {
    //                var output = Dags.ProcessItems(grod, parsed);
    //                RenderOutput(grod, output);
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        WriteOutput($"{NL}Fatal error: {ex.Message}");
    //    }
    //    OutStream?.Close();
    //}

    /*
    public void Input(string input)
    {
    }


    // Wrap the event in a protected virtual method
    // to enable derived classes to raise the event.
    protected virtual void RaiseOutputEvent(string text)
    {
        // Raise the event in a thread-safe manner using the ?. operator.
        OutputEvent?.Invoke(this, new OutputEventArgs(text));
    }
    */
}

public class OutputEventArgs(string text)
{
    public string Text { get; } = text;
}
