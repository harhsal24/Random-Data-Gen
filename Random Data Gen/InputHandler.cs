public static class InputHandler
{
    public static void HandleManualInput()
    {
        Console.WriteLine("Enter the number of fields you want to generate:");
        if (!int.TryParse(Console.ReadLine(), out int fieldCount))
        {
            Console.WriteLine("Invalid number. Please run the program again.");
            return;
        }

        var properties = new List<(string Name, string Type)>();

        for (int i = 0; i < fieldCount; i++)
        {
            Console.WriteLine($"Field {i + 1}:");

            Console.Write("Name: ");
            string name = Console.ReadLine()?.Trim();

            // Check for duplicate names
            if (properties.Any(p => p.Name == name))
            {
                Console.WriteLine("Field name already exists. Please enter a unique field name.");
                i--;  // Repeat iteration for duplicate field.
                continue;
            }

            Console.Write("Type (string/int/double/decimal/datetime/dateonly/timeonly/datetimeoffset/timespan): ");
            string type = Console.ReadLine()?.Trim().ToLower();

            // Validate input type
            if (!IsSimpleType(type) && !IsSpecialDateOrTimeType(type))
            {
                Console.WriteLine("Invalid type entered. Please enter a valid type.");
                i--;  // Repeat iteration for invalid type.
                continue;
            }

            properties.Add((name, type));
            Console.WriteLine($"Field added: {name} ({type})");
        }

        Console.WriteLine("All fields added. Ready for data generation.");
        // Pass the properties list to your data generation method
    }

    public static void HandleClassInput()
    {
        Console.WriteLine("Paste your class or enum definition below (press Enter twice to finish):");
        string typeContent = ReadMultiLineInput().Trim();

        if (string.IsNullOrWhiteSpace(typeContent))
        {
            Console.WriteLine("No input detected. Please provide a valid class or enum definition.");
            return;
        }

        try
        {
            var (typeName, typeKind, properties) = ClassParser.ParseType(typeContent);

            // Validate type
            if (!IsValidType(typeKind))
            {
                Console.WriteLine($"Invalid type detected: {typeKind}. Please provide a valid class, enum, or record definition.");
                return;
            }

            // Validate properties
            if (properties == null || properties.Count == 0)
            {
                Console.WriteLine("No valid properties found in the input. Please provide a valid class, enum, or record definition.");
                return;
            }

            // If valid, append to file
            FileHandler.AppendToFile(typeContent);

            Console.WriteLine($"\n{typeKind.ToUpperInvariant()} name: {typeName}");
            Console.WriteLine("Properties found:");

            // Print all properties, not just nested or complex ones
            foreach (var prop in properties)
            {
                Console.WriteLine($"- {prop.Name}: {prop.Type}");
            }

            // Handle nested types
            HandleNestedTypes(properties);

            Console.WriteLine("\nReady for data generation based on these types.");
            // Here you would pass the types and properties to your data generation method
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }


    private static void HandleNestedTypes(List<(string Name, string Type)> properties)
    {
        var nestedTypes = properties
            .Select(p => p.Type)
            .Distinct()
            .Where(t => !IsSimpleType(t) && !IsSpecialDateOrTimeType(t)) // Exclude special types
            .ToList();

        foreach (var nestedType in nestedTypes)
        {
            var typeToProcess = ExtractInnerType(nestedType);

            if (!IsSimpleType(typeToProcess) && !IsSpecialDateOrTimeType(typeToProcess))
            {
                Console.WriteLine($"\nPlease provide the definition for the nested type '{typeToProcess}':");
                string nestedTypeContent = ReadMultiLineInput();
                var (nestedTypeName, nestedTypeKind, nestedProperties) = ClassParser.ParseType(nestedTypeContent);

                if (!IsValidType(nestedTypeKind))
                {
                    Console.WriteLine("Invalid nested type definition. Please try again.");
                    continue;
                }

                Console.WriteLine($"\nNested {nestedTypeKind.ToUpperInvariant()} name: {nestedTypeName}");
                Console.WriteLine($"{(nestedTypeKind == "enum" ? "Values" : "Properties")} found:");
                foreach (var prop in nestedProperties)
                {
                    Console.WriteLine($"- {prop.Name}: {prop.Type}");
                }
            }
        }
    }

    private static bool IsSimpleType(string type)
    {
        var simpleTypes = new HashSet<string> { "int", "string", "bool", "double", "decimal" };
        return simpleTypes.Contains(type.ToLower());
    }

    private static void HandleSpecialDateTimeType(string type)
    {
        Console.WriteLine($"Do you want to treat {type} as a string? (yes/no)");
        string choice = Console.ReadLine()?.Trim().ToLower();

        if (choice == "yes" || choice == "y")
        {
            Configuration.TreatAsString = true;

            Console.WriteLine("Enter the custom format (optional, leave empty for default):");
            string format = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(format))
            {
                Configuration.SetCustomFormat(type, format);
                Console.WriteLine($"Custom format set for {type}: {format}");
            }
            else
            {
                Console.WriteLine($"Default format will be used for {type}");
            }
        }
        else
        {
            Configuration.TreatAsString = false;
            Console.WriteLine($"{type} will not be treated as a string.");
        }

        Console.WriteLine($"Treat as string: {Configuration.TreatAsString}");
    }

    private static bool IsSpecialDateOrTimeType(string type)
    {
        var specialTypes = new HashSet<string>
        {
            "datetime", "dateonly", "timeonly", "datetimeoffset", "timespan"
        };
        return specialTypes.Contains(type.ToLower());
    }

    private static string ExtractInnerType(string type)
    {
        if (type.StartsWith("list<", StringComparison.OrdinalIgnoreCase) ||
            type.StartsWith("ienumerable<", StringComparison.OrdinalIgnoreCase))
        {
            return type.Substring(type.IndexOf('<') + 1, type.IndexOf('>') - type.IndexOf('<') - 1);
        }
        return type;
    }

    private static string ReadMultiLineInput()
    {
        var inputLines = new List<string>();
        string line;
        while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
        {
            inputLines.Add(line);
        }

        return string.Join(Environment.NewLine, inputLines).Trim();
    }

    private static bool IsValidType(string typeKind)
    {
        // Check if the type is a valid class, enum, or record
        var validTypes = new HashSet<string> { "class", "enum", "record" };
        return validTypes.Contains(typeKind);
    }
}

public static class FileHandler
{
    public static void AppendToFile(string content, string fileName = "UserClass.txt")
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

        try
        {
            // Clear the content of the file if it exists, or create a new file
            File.WriteAllText(filePath, string.Empty);
            Console.WriteLine("File created or cleared successfully.");

            // Append the new content
            File.AppendAllText(filePath, content + Environment.NewLine);
            Console.WriteLine("Content appended successfully.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An I/O error occurred while writing to the file: {ex.Message}");
            Console.WriteLine("Please ensure the file is not in use and try again.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}

