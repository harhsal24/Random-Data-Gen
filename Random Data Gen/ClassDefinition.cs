public class ClassDefinition
{
    public string Name { get; }
    public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();

    public ClassDefinition(string name)
    {
        Name = name;
    }
}
 