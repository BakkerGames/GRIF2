namespace Grif;

public enum OutputMessageType
{
    Unknown = 0,
    Text = 1,
    Command = 2,
}

public class OutputMessage
{
    public OutputMessageType MessageType { get; set; }

    public string Content { get; set; } = string.Empty;

    public string ExtraData { get; set; } = string.Empty;
}
