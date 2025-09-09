namespace Grif;

public static class Common
{
    public static readonly StringComparison OIC = StringComparison.OrdinalIgnoreCase;

    public const string NULL = "null";
    public const string TRUE = "true";
    public const string FALSE = "false";

    public const string NL = "\\n";
    public const string TAB = "\\t";
    public const string SPACE = "\\s";
    public const string COMMA_UTF = "\\u002c";

    public const string INCHANNEL = "#INCHANNEL;";

    // OutChannel constants used by GRIF

    public const string OUTCHANNEL_ASK = "#ASK;";
    public const string OUTCHANNEL_ENTER = "#ENTER;";
    public const string OUTCHANNEL_EXISTS_SAVE = "#EXISTS;";
    public const string OUTCHANNEL_EXISTS_SAVE_NAME = "#EXISTSNAME;";
    public const string OUTCHANNEL_GAMEOVER = "#GAMEOVER;";
    public const string OUTCHANNEL_RESTART = "#RESTART;";
    public const string OUTCHANNEL_RESTORE = "#RESTORE;";
    public const string OUTCHANNEL_RESTORE_NAME = "#RESTORENAME;";
    public const string OUTCHANNEL_SAVE = "#SAVE;";
    public const string OUTCHANNEL_SAVE_NAME = "#SAVENAME;";
    public const string OUTCHANNEL_SLEEP = "#SLEEP;";
}
