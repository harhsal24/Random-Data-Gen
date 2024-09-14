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
        foreach (var classDef in TrackedClasses)
        {
            Console.WriteLine($"\nClass: {classDef.Name}");

            // Check if it's an enum
            if (classDef.Properties.Any() && classDef.Properties[0].Type.ToLower() == "enum")
            {
                Console.WriteLine("Enum Values:");
                foreach (var value in classDef.Properties[0].Values)
                {
                    Console.WriteLine($"  - {value}");
                }
            }
            else
            {
                foreach (var prop in classDef.Properties)
                {
                    if (prop.Type.ToLower() == "enum" && prop.Values != null && prop.Values.Any())
                    {
                        Console.WriteLine($"- {prop.Name} (Enum):");
                        foreach (var value in prop.Values)
                        {
                            Console.WriteLine($"  - {value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"- {prop.Name}: {prop.Type}");
                    }
                }
            }
        }
    }
}



