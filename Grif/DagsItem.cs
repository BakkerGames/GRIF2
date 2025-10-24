namespace Grif;

public enum DagsType
{
    Error = -1,
    Text = 0,
    Internal = 1,
    OutChannel = 2,
    InChannel = 3,
}

public record DagsItem(DagsType Type, string Value);
