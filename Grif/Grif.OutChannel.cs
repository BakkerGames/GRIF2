using static Grif.Common;

namespace Grif;

public static partial class Grif
{
    public static void HandleOutChannel(Grod grod, List<DagsItem> items, ref int index)
    {
        var item = items[index];
        if (item.Value.Equals(OUTCHANNEL_GAMEOVER, OIC))
        {
            GameOver = true;
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_EXISTS_SAVE, OIC))
        {
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_EXISTS_SAVE_NAME, OIC))
        {
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_SAVE, OIC))
        {
            var savefile = GetSavePath(grod, SAVE_FILENAME, SAVE_EXTENSION);
            var itemList = grod.Items(false, true);
            WriteGrif(savefile, itemList, true);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_SAVE_NAME, OIC))
        {
            // savename is the second item
            index++;
            if (index >= items.Count)
            {
                throw new Exception("Save file name not supplied.");
            }
            var item2 = items[index];
            if (item2.Type != DagsType.Text && item2.Type != DagsType.Internal && item2.Type != DagsType.OutChannel)
            {
                throw new Exception("Invalid type for save file name");
            }
            var savename = item2.Value;
            var savefile = GetSavePath(grod, savename, SAVE_EXTENSION);
            var itemList = grod.Items(false, true);
            WriteGrif(savefile, itemList, false);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTORE, OIC))
        {

            var savefile = GetSavePath(grod, SAVE_FILENAME, SAVE_EXTENSION);
            if (!File.Exists(savefile))
            {
                throw new FileNotFoundException(savefile);
            }
            var itemList = ReadGrif(savefile);
            grod.Clear(false); // clear only the user data
            grod.AddItems(itemList);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTORE_NAME, OIC))
        {
            // savename is the second item
            index++;
            if (index >= items.Count)
            {
                throw new Exception("Save file name not supplied.");
            }
            var item2 = items[index];
            if (item2.Type != DagsType.Text && item2.Type != DagsType.Internal && item2.Type != DagsType.OutChannel)
            {
                throw new Exception("Invalid type for save file name");
            }
            var savename = item2.Value;
            var savefile = GetSavePath(grod, savename, SAVE_EXTENSION);
            if (!File.Exists(savefile))
            {
                throw new FileNotFoundException(savefile);
            }
            var itemList = ReadGrif(savefile);
            grod.Clear(false); // clear only the user data
            grod.AddItems(itemList);
            return;
        }
        if (item.Value.Equals(OUTCHANNEL_RESTART, OIC))
        {
            grod.Clear(false); // clear only the user data
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
        if (item.Value.Equals(OUTCHANNEL_ENTER, OIC))
        {
            Prompt(grod);
            _ = GetInput(grod);
            AfterPrompt(grod);
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
