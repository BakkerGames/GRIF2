namespace Grif;

public enum InputMessageType
{
    Unknown = 0,
    Text = 1,
    Script = 2,
}

public class InputMessage
{
    public InputMessageType MessageType { get; set; }

    public string Content { get; set; } = string.Empty;
}
