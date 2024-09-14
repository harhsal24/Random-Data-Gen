public class PropertyDefinition
{
    public string Name { get; }
    public string Type { get; }
    public List<string> Values { get; set; } // Store enum values

    public PropertyDefinition(string name, string type, List<string> values = null)
    {
        Name = name;
        Type = type;
        Values = values;
    }
}
