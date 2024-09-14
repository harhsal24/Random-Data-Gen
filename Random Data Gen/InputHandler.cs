public static class InputHandler
{
    public static void HandleManualInput()
    {
        Console.WriteLine("Enter the number of fields you want to generate:");
        if (!int.TryParse(Console.ReadLine(), out int fieldCount) || fieldCount <= 0)
        {
            Console.WriteLine("Invalid number. Please enter a positive integer.");
            return;
        }

        var properties = new List<(string Name, string Type)>();

        for (int i = 0; i < fieldCount; i++)
        {
            Console.WriteLine($"Field {i + 1}:");

            string name = GetValidFieldName();
            string type = GetValidFieldType();

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
            if (!IsValidType(typeKind))
            {
                Console.WriteLine($"Invalid type detected: {typeKind}. Please provide a valid class, enum, or record definition.");
                return;
            }
            if (properties == null || properties.Count == 0)
            {
                Console.WriteLine("No valid properties found in the input. Please provide a valid class, enum, or record definition.");
                return;
            }
            var classDef = new ClassDefinition(typeName);
            if (typeKind.ToLower() == "enum")
            {
                // For enums, add a single property with all enum values
                var enumValues = properties.Select(p => p.Name).ToList();
                classDef.Properties.Add(new PropertyDefinition(typeName, typeKind, enumValues));
                Console.WriteLine($"Enum {typeName} with values:");
                foreach (var value in enumValues)
                {
                    Console.WriteLine($"- {value}");
                }
            }
            else
            {
                foreach (var prop in properties)
                {
                    Console.WriteLine($"- {prop.Name}: {prop.Type}");
                    classDef.Properties.Add(new PropertyDefinition(prop.Name, prop.Type));
                }
            }
            Program.TrackedClasses.Add(classDef);
            FileHandler.AppendToFile(typeContent);
            HandleNestedTypes(properties);
            Console.WriteLine("\nReady for data generation based on these types.");
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
            .Where(t => !IsSimpleType(t) && !IsSpecialDateOrTimeType(t))
            .ToList();
        foreach (var nestedType in nestedTypes)
        {
            var typeToProcess = ExtractInnerType(nestedType);
            if (!IsSimpleType(typeToProcess) && !IsSpecialDateOrTimeType(typeToProcess))
            {
                Console.WriteLine($"\nPlease provide the definition for the nested type '{typeToProcess}':");
                string nestedTypeContent = ReadMultiLineInput();
                var (nestedTypeName, nestedTypeKind, nestedProperties) = ClassParser.ParseType(nestedTypeContent);
                Console.WriteLine($"\nNested {nestedTypeKind.ToUpperInvariant()} name: {nestedTypeName}");
                var nestedClassDef = new ClassDefinition(nestedTypeName);
                if (nestedTypeKind.ToLower() == "enum")
                {
                    // For enums, add a single property with all enum values
                    var enumValues = nestedProperties.Select(p => p.Name).ToList();
                    nestedClassDef.Properties.Add(new PropertyDefinition(nestedTypeName, nestedTypeKind, enumValues));
                    Console.WriteLine("Enum values:");
                    foreach (var value in enumValues)
                    {
                        Console.WriteLine($"- {value}");
                    }
                }
                else
                {
                    Console.WriteLine("Properties found:");
                    foreach (var prop in nestedProperties)
                    {
                        Console.WriteLine($"- {prop.Name}: {prop.Type}");
                        nestedClassDef.Properties.Add(new PropertyDefinition(prop.Name, prop.Type));
                    }
                }
                Program.TrackedClasses.Add(nestedClassDef);
            }
        }
    }

    private static bool IsSimpleType(string type)
    {
        var simpleTypes = new HashSet<string> { "int", "string", "bool", "double", "decimal", "float", "long", "short", "byte", "char" };
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
            return type.Substring(type.IndexOf('<') + 1, type.LastIndexOf('>') - type.IndexOf('<') - 1);
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
        var validTypes = new HashSet<string> { "class", "enum", "record" };
        return validTypes.Contains(typeKind.ToLower());
    }

    private static string GetValidFieldName()
    {
        string name;
        do
        {
            Console.Write("Name: ");
            name = Console.ReadLine()?.Trim();
        } while (string.IsNullOrWhiteSpace(name));
        return name;
    }

    private static string GetValidFieldType()
    {
        string type;
        var validTypes = new HashSet<string> { "string", "int", "double", "decimal", "datetime", "dateonly", "timeonly", "datetimeoffset", "timespan", "list", "ienumerable" };
        do
        {
            Console.Write("Type (string/int/double/decimal/datetime/dateonly/timeonly/datetimeoffset/timespan/list/ienumerable): ");
            type = Console.ReadLine()?.Trim().ToLower();
        } while (!validTypes.Contains(type));
        return type;
    }
}