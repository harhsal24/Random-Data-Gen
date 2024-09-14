using System;

public static class Program
{
    public static List<ClassDefinition> TrackedClasses = new List<ClassDefinition>();

    public static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Random Data Generator!");
        Console.WriteLine("Choose an input method:");
        Console.WriteLine("1. Manual field input");
        Console.WriteLine("2. Class input (paste)");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                InputHandler.HandleManualInput();
                break;
            case "2":
                InputHandler.HandleClassInput();
                break;
            default:
                Console.WriteLine("Invalid choice. Please run the program again.");
                break;
        }

        // Print tracked classes and properties
        PrintTrackedClasses();
    }

    private static void PrintTrackedClasses()
    {
        Console.WriteLine("\nTracked Classes:");

        foreach (var classDef in Program.TrackedClasses)
        {
            Console.WriteLine($"\nClass: {classDef.Name}");

            foreach (var prop in classDef.Properties)
            {
                Console.WriteLine($"- {prop.Name}: {prop.Type}");
            }
        }
    }
}


public class ClassDefinition
{
    public string Name { get; }
    public List<PropertyDefinition> Properties { get; } = new List<PropertyDefinition>();

    public ClassDefinition(string name)
    {
        Name = name;
    }
}

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