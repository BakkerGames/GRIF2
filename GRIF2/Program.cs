using GROD2;

namespace GRIF2;

internal class Program
{
    static void Main(string[] _)
    {
        var grod = new Grod2();
        grod.AddLevel("base");
        grod.AddLevel("overlay", "base");
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
                grod.Set("base", key, value);
            }
        }
        foreach (string key2 in grod.GetKeys("base"))
        {
            Console.WriteLine($"{key2}: {grod.Get("base", key2)}");
        }
    }
}
