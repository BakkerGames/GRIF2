using static Grif.Common;
using static Grif.Parser;

namespace Grif;

public delegate void InputEventHandler(object sender);
public delegate void OutputEventHandler(object sender, OutputMessage e);

public class Game
{
    private Grod _baseGrod = new("");
    private Grod _overlayGrod = new("");
    private string? _saveBasePath = "";
    private string? _referenceBasePath = "";

    private const string SCRIPT = "@script(";
    private const string BACKGROUND_PREFIX = "background.";

    public event InputEventHandler? InputEvent;
    public event OutputEventHandler? OutputEvent;

    public Queue<InputMessage> InputMessages { get; } = new();

    public Queue<OutputMessage> OutputMessages { get; } = new();

    public void Initialize(Grod grod, string saveBasePath, string? referenceBasePath = null)
    {
        _saveBasePath = saveBasePath;
        _referenceBasePath = referenceBasePath;
        try
        {
            if (!Path.IsPathRooted(_saveBasePath))
            {
                _saveBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), APP_NAME, _saveBasePath);
            }
            if (!Directory.Exists(_saveBasePath))
            {
                Directory.CreateDirectory(_saveBasePath);
            }
        }
        catch (Exception)
        {
            throw new IOException("Failed to initialize game save path.");
        }
        try
        {
            if (!string.IsNullOrEmpty(_referenceBasePath))
            {
                if (!Path.IsPathRooted(_referenceBasePath))
                {
                    _referenceBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_NAME, _referenceBasePath);
                }
                if (!Directory.Exists(_referenceBasePath))
                {
                    throw new IOException("Reference path does not exist.");
                }
            }
        }
        catch (IOException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new IOException("Failed to initialize game resource path.");
        }
        try
        {
            _baseGrod = grod;
            _overlayGrod = new(Path.Combine(_saveBasePath, SAVE_FILENAME + SAVE_EXTENSION))
            {
                Parent = _baseGrod
            };
        }
        catch (Exception)
        {
            throw new IOException("Failed to initialize game data.");
        }
    }

    public async Task GameLoop()
    {
        while (true)
        {
            if (GameOver())
            {
                break;
            }
            await AdvanceGameState();
            if (OutputMessages.Count > 0)
            {
                var outputMessage = OutputMessages.Dequeue();
                ProcessOutputMessage(outputMessage);
            }
            if (GameOver())
            {
                break;
            }
            if (InputEvent != null && InputMessages.Count == 0)
            {
                InputEvent?.Invoke(this);
            }
            while (InputMessages.Count == 0)
            {
                await Task.Delay(100); // TODO adjust delay as needed
            }
            if (InputMessages.Count > 0)
            {
                var inputMessage = InputMessages.Dequeue();
                await ProcessInputMessage(inputMessage);
            }
            if (OutputMessages.Count > 0)
            {
                var outputMessage = OutputMessages.Dequeue();
                ProcessOutputMessage(outputMessage);
            }
        }
    }

    private void ProcessOutputMessage(OutputMessage outputMessage)
    {
        switch (outputMessage.MessageType)
        {
            case OutputMessageType.Text:
                OutputEvent?.Invoke(this, outputMessage);
                break;
            default:
                OutputEvent?.Invoke(this, new OutputMessage
                {
                    MessageType = OutputMessageType.Text,
                    Content = $"Unknown Output Message Type: {outputMessage.MessageType}",
                    ExtraData = (outputMessage.Content + " " + outputMessage.ExtraData).Trim()
                });
                break;
        }
    }

    /// <summary>
    /// Indicates if the game has ended.
    /// </summary>
    public bool GameOver()
    {
        if (_overlayGrod.GetBool(GAMEOVER, true) ?? false)
        {
            return true;
        }
        return false;
    }

    private async Task ProcessInputMessage(InputMessage inputMessage)
    {
        var inputItems = await Task.FromResult(ParseInput(_overlayGrod, inputMessage.Content));
        if (inputItems != null)
        {
            var outputItems = Dags.ProcessItems(_overlayGrod, inputItems);
            foreach (var item in outputItems)
            {
                OutputMessages.Enqueue(new OutputMessage
                {
                    // TODO other types
                    MessageType = OutputMessageType.Text,
                    Content = item.Value
                });
            }
        }
    }

    private async Task AdvanceGameState()
    {
        var keys = await Task.FromResult(_overlayGrod.Keys(BACKGROUND_PREFIX, true, false));
        foreach (var key in keys)
        {
            var script = $"{SCRIPT}{key})";
            var items = await Task.FromResult(Dags.Process(_overlayGrod, script));
            foreach (var item in items)
            {
                OutputMessages.Enqueue(new OutputMessage
                {
                    MessageType = OutputMessageType.Text,
                    Content = item.Value
                });
            }
        }
    }
}
