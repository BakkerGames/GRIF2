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

    public const string APP_NAME = "GRIF";
    public const string DATA_EXTENSION = ".grif";
    public const string SAVE_FILENAME = "save";
    public const string SAVE_EXTENSION = ".grifsave";

    public const string AFTER_PROMPT = "system.after_prompt";
    public const string DONT_UNDERSTAND = "system.dont_understand";
    public const string GAMENAME = "system.gamename";
    public const string INTRO = "system.intro";
    public const string OUTPUT_WIDTH = "system.output_width";
    public const string PROMPT = "system.prompt";
    public const string UPPERCASE = "system.uppercase";
    public const string DEBUG = "system.debug";
    public const string WORDSIZE = "system.wordsize";

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
