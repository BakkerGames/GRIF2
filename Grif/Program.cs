namespace Grif;

internal class Program
{
    static void Main(string[] _)
    {
        var grodBase = new Grod("base");
        //var grodOverlay = new Grod("overlay", grodBase);
        var lines = File.ReadAllLines("..\\..\\..\\..\\TestData\\test.grif");
        var key = string.Empty;
        foreach (var line in lines)
        {
            if (!line.StartsWith('\t'))
            {
                key = line.Trim();
            }
            else
            {
                var value = line.Trim();
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    continue; // Skip empty keys or values
                }
                grodBase.Set( key, value);
            }
        }
        foreach (GrodItem item in grodBase.Items(false, true))
        {
            Console.WriteLine($"{item.Key}: {grodBase.Get(item.Key, false)}");
        }
    }
}
