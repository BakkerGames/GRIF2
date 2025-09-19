namespace Grif;

internal class Program
{
    static void Main(string[] args)
    {
        string filename;
        if (args.Length == 0)
        {
            Console.WriteLine("Please specify a GRIF filename.");
            return;
        }
        filename = args[0];
        if (!File.Exists(filename))
        {
            Console.WriteLine($"File not found: {filename}");
            return;
        }
        var grodBase = new Grod("base");
        List<GrodItem> items = Grif.ReadGrif(filename);
        grodBase.AddItems(items);
        var grod = new Grod("overlay", grodBase);
        Grif.RunGame(grod);
    }
}
