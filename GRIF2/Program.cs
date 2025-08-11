using GROD2;

namespace GRIF2;

internal class Program
{
    static void Main(string[] _)
    {
        var grod = new Grod2();
        grod.AddLevel("base");
        grod.AddLevel("overlay", "base");
    }
}
