namespace Grif;

public enum MessageType
{
    Error = -1,
    Unknown = 0,
    Text = 1,
    Script = 2,
    Internal = 3,
    OutChannel = 4,
    InChannel = 5,
}

public record Message(MessageType Type, string Value, string? ExtraValue = null);
