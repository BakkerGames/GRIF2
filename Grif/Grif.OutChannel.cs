using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    public static void HandleOutChannel(Grod grod, DagsItem item)
    {
        if (item.Value.Equals(OUTCHANNEL_GAMEOVER, OIC))
        {
            GameOver = true;
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_ASK, OIC))
        {
            Prompt(grod);
            var input = GetInput(grod);
            AfterPrompt(grod);
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
            RenderOutput(grod, output);
            return;
        }
        throw new Exception($"Unknown OutChannel command {item.Value}");
    }
}
