public class PropertyDefinition
{
    public string Name { get; }
    public string Type { get; }

    public PropertyDefinition(string name, string type)
    {
        Name = name;
        Type = type;
    }
}
