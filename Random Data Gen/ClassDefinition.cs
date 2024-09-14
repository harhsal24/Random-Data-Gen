public class ClassDefinition
{
    public string Name { get; }
    public List<PropertyDefinition> Properties { get; } = new List<PropertyDefinition>();

    public ClassDefinition(string name)
    {
        Name = name;
    }
}
 