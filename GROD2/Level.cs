namespace GROD2;

public class Level : Data
{
    public Level(string name)
    {
        Name = name;
    }

    public Level(string name, string? parent)
    {
        Name = name;
        Parent = parent;
    }

    public string Name { get; set; }

    public string? Parent { get; set; }
}
