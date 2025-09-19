using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    public static void HandleOutChannel(Grod grod, DagsItem item, int outputWidth, ref int currPos, ref bool gameOver)
    {
        if (item.Value.Equals(OUTCHANNEL_GAMEOVER, OIC))
        {
            gameOver = true;
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_ASK, OIC))
        {
            Prompt(grod, outputWidth, ref currPos, ref gameOver);
            var input = Console.ReadLine() ?? "";
            AfterPrompt(grod, outputWidth, ref currPos, ref gameOver);
            if (!Dags.IsNull(grod.Get(INCHANNEL, false)))
            {
                throw new Exception("DagsInChannel value is not empty.");
            }
            grod.Set(INCHANNEL, input);
            return;
        }
        if (item.Value.StartsWith('@'))
        {
            var output = Dags.ProcessItems(grod, [new DagsItem(DagsType.Internal, item.Value)]);
            RenderOutput(grod, output, outputWidth, ref currPos, ref gameOver);
            return;
        }
        throw new Exception($"Unknown OutChannel command {item.Value}");
    }
}
