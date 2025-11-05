using static Grif.Common;
using static Grif.IO;
using static Grif.Parser;

namespace Grif;

public delegate void InputEventHandler(object sender);
public delegate void OutputEventHandler(object sender, Message e);

public class Game
{
    public static string Version { get { return "2.2025.1104"; } }

    private Grod _baseGrod = new("");
    private Grod _overlayGrod = new("");
    private string _saveBasePath = "";
    private string? _referenceBasePath;

    private const string SCRIPT = "@script(";
    private const string BACKGROUND_PREFIX = "background.";

    public event InputEventHandler? InputEvent;
    public event OutputEventHandler? OutputEvent;

    public Queue<Message> InputMessages { get; } = new();

    public Queue<Message> OutputMessages { get; } = new();

    public void Initialize(Grod grod, string saveBasePath, string? referenceBasePath = null)
    {
        GameOver = false;
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
        try
        {
            ParseInit(_overlayGrod);
        }
        catch (Exception)
        {
            throw new SystemException("Failed to initialize parser.");
        }
    }

    public async Task GameLoop()
    {
        while (true)
        {
            if (GameOver)
            {
                break;
            }
            AdvanceGameState();
            while (OutputMessages.Count > 0)
            {
                var outputMessage = OutputMessages.Dequeue();
                ProcessOutputMessage(outputMessage);
            }
            if (GameOver)
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
                ProcessInputMessage(inputMessage);
            }
            while (OutputMessages.Count > 0)
            {
                var outputMessage = OutputMessages.Dequeue();
                ProcessOutputMessage(outputMessage);
            }
        }
    }

    public bool GameOver { get; set; } = false;

    public string? Prompt()
    {
        var prompt = _overlayGrod.Get(PROMPT, true);
        if (prompt != null && prompt.StartsWith('@'))
        {
            prompt = Dags.Process(_overlayGrod, prompt).FirstOrDefault()?.Value;
        }
        return prompt;
    }

    public string? AfterPrompt()
    {
        var afterPrompt = _overlayGrod.Get(AFTER_PROMPT, true);
        if (afterPrompt != null && afterPrompt.StartsWith('@'))
        {
            afterPrompt = Dags.Process(_overlayGrod, afterPrompt).FirstOrDefault()?.Value;
        }
        return afterPrompt;
    }

    private void ProcessInputMessage(Message inputMessage)
    {
        var inputItems = ParseInput(_overlayGrod, inputMessage.Value);
        foreach (var item in inputItems ?? [])
        {
            OutputMessages.Enqueue(new Message(item.Type, item.Value, item.ExtraValue));
        }
    }

    private void ProcessOutputMessage(Message message)
    {
        switch (message.Type)
        {
            case MessageType.Text:
                OutputEvent?.Invoke(this, message);
                break;
            case MessageType.OutChannel:
                HandleOutChannel(message);
                break;
            case MessageType.Script:
                var outputItems = Dags.ProcessItems(_overlayGrod, [message]);
                foreach (var item in outputItems)
                {
                    OutputMessages.Enqueue(item);
                }
                break;
            default:
                throw new Exception($"Unsupported output message type: {message.Type}");
        }
    }

    private void AdvanceGameState()
    {
        var keys = _overlayGrod.Keys(BACKGROUND_PREFIX, true, false);
        foreach (var key in keys)
        {
            var script = $"{SCRIPT}{key})";
            var items = Dags.Process(_overlayGrod, script);
            foreach (var item in items)
            {
                OutputMessages.Enqueue(item);
            }
        }
    }

    public void HandleOutChannel(Message item)
    {
        bool exists;
        if (item.Value.Equals(OUTCHANNEL_GAMEOVER, OIC))
        {
            GameOver = true;
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_EXISTS_SAVE, OIC))
        {
            var savefile = Path.Combine(_saveBasePath, SAVE_FILENAME + SAVE_EXTENSION);
            exists = File.Exists(savefile);
            _overlayGrod.Set(INCHANNEL, exists ? "true" : "false");
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_EXISTS_SAVE_NAME, OIC))
        {
            if (item.ExtraValue == null)
            {
                throw new Exception("Save filename not specified.");
            }
            var savefile = Path.Combine(_saveBasePath, item.ExtraValue + SAVE_EXTENSION);
            exists = File.Exists(savefile);
            _overlayGrod.Set(INCHANNEL, exists ? "true" : "false");
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_SAVE, OIC))
        {
            var savefile = Path.Combine(_saveBasePath, SAVE_FILENAME + SAVE_EXTENSION);
            var itemList = _overlayGrod.Items(false, true);
            WriteGrif(savefile, itemList, true);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_SAVE_NAME, OIC))
        {
            if (item.ExtraValue == null)
            {
                throw new Exception("Save filename not specified.");
            }
            var savefile = Path.Combine(_saveBasePath, item.ExtraValue + SAVE_EXTENSION);
            var itemList = _overlayGrod.Items(false, true);
            WriteGrif(savefile, itemList, false);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTORE, OIC))
        {
            var savefile = Path.Combine(_saveBasePath, SAVE_FILENAME + SAVE_EXTENSION);
            if (!File.Exists(savefile))
            {
                throw new FileNotFoundException(savefile);
            }
            var itemList = ReadGrif(savefile);
            _overlayGrod.Clear(false); // clear only the user data
            _overlayGrod.AddItems(itemList);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTORE_NAME, OIC))
        {
            if (item.ExtraValue == null)
            {
                throw new Exception("Save filename not specified.");
            }
            var savefile = Path.Combine(_saveBasePath, item.ExtraValue + SAVE_EXTENSION);
            if (!File.Exists(savefile))
            {
                throw new FileNotFoundException(savefile);
            }
            var itemList = ReadGrif(savefile);
            _overlayGrod.Clear(false); // clear only the user data
            _overlayGrod.AddItems(itemList);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTART, OIC))
        {
            _overlayGrod.Clear(false); // clear only the user data
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_ASK, OIC))
        {
            if (InputEvent != null && InputMessages.Count == 0)
            {
                InputEvent?.Invoke(this);
            }
            while (InputMessages.Count == 0)
            {
                Thread.Sleep(100); // TODO adjust delay as needed
            }
            var inputMessage = InputMessages.Dequeue();
            _overlayGrod.Set(INCHANNEL, inputMessage.Value);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_ENTER, OIC))
        {
            if (InputEvent != null && InputMessages.Count == 0)
            {
                InputEvent?.Invoke(this);
            }
            while (InputMessages.Count == 0)
            {
                Thread.Sleep(100); // TODO adjust delay as needed
            }
            _ = InputMessages.Dequeue();
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_SLEEP, OIC))
        {
            if (!int.TryParse(item.ExtraValue, out int value))
            {
                throw new Exception($"Invalid sleep duration: {item.ExtraValue}");
            }
            Thread.Sleep(value);
            return;
        }
        if (item.Value.StartsWith('@'))
        {
            var outputItems = Dags.ProcessItems(_overlayGrod, [new Message(MessageType.Script, item.Value)]);
            foreach (var outputItem in outputItems)
            {
                OutputMessages.Enqueue(outputItem);
            }
            return;
        }
        if (item.Value.StartsWith('#') && item.Value.EndsWith(';'))
        {
            // future enhancements go here, just quietly ignore for now
            return;
        }
        throw new Exception($"Unknown OutChannel command {item.Value}");
    }
}
