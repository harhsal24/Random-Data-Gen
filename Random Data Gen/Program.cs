using System;
using System.Text.Json;

public static class Program
{
    public static List<ClassDefinition> TrackedClasses = new List<ClassDefinition>();

    public static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Random Data Generator!");
        Console.WriteLine("Choose an input method:");
        Console.WriteLine("1. Manual field input");
        Console.WriteLine("2. Class input (paste)");

        //string choice = Console.ReadLine();

        //switch (choice)
        //{
        //    case "1":
        //        InputHandler.HandleManualInput();
        //        break;
        //    case "2":
        //        InputHandler.HandleClassInput();
        //        break;
        //    default:
        //        Console.WriteLine("Invalid choice. Please run the program again.");
        //        break;
        //}

        InitializeTrackedClasses();
        // Print tracked classes and properties
        PrintTrackedClasses();

        // Assuming you have a list of ClassDefinitions called trackedClasses
        foreach (var classDef in TrackedClasses)
        {
            DataGenerator.GenerateData(classDef, TrackedClasses, 3); // Generate 3 instances of each class
        }
        Console.WriteLine("TrackedClasses:");
        Console.WriteLine(JsonSerializer.Serialize(TrackedClasses));
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

    public static void InitializeTrackedClasses()
    {
        TrackedClasses.Add(new ClassDefinition("Order")
        {
            Properties = new List<PropertyDefinition>
            {
                new PropertyDefinition("OrderId", "int"),
                new PropertyDefinition("CustomerName", "string"),
                new PropertyDefinition("OrderDate", "DateTime"),
                new PropertyDefinition("TotalAmount", "decimal"),
                new PropertyDefinition("IsShipped", "bool"),
                new PropertyDefinition("Items", "List<OrderItem>"),
                new PropertyDefinition("Status", "OrderStatus"),
                new PropertyDefinition("ShippingAddress", "Address")
            }
        });

        TrackedClasses.Add(new ClassDefinition("OrderItem")
        {
            Properties = new List<PropertyDefinition>
            {
                new PropertyDefinition("ProductId", "int"),
                new PropertyDefinition("ProductName", "string"),
                new PropertyDefinition("Quantity", "int"),
                new PropertyDefinition("UnitPrice", "decimal")
            }
        });

        TrackedClasses.Add(new ClassDefinition("OrderStatus")
        {
            Properties = new List<PropertyDefinition>
            {
                new PropertyDefinition("OrderStatus", "enum", new List<string> { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" })
            }
        });

        TrackedClasses.Add(new ClassDefinition("Address")
        {
            Properties = new List<PropertyDefinition>
            {
                new PropertyDefinition("Street", "string"),
                new PropertyDefinition("City", "string"),
                new PropertyDefinition("State", "string"),
                new PropertyDefinition("ZipCode", "string"),
                new PropertyDefinition("Country", "string")
            }
        });
    }
}




