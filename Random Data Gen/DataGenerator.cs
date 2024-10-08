﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

public class DataGenerator
{
    private static readonly Random random = new Random();

    public static object GenerateRandomValue(PropertyDefinition property, List<ClassDefinition> trackedClasses)
    {
        if (property.Type.StartsWith("List<") || property.Type.StartsWith("IEnumerable<"))
        {
            return GenerateRandomList(property, trackedClasses);
        }

        if (IsEnum(property.Type, trackedClasses))
        {
            return GenerateRandomEnum(property, trackedClasses);
        }

        if (property.Type.ToLower() == "enum" && property.Values != null && property.Values.Any())
        {
            return property.Values[random.Next(property.Values.Count)];
        }

        switch (property.Type.ToLower())
        {
            case "int":
                return random.Next(1, 1000000);  // More reasonable range for OrderId and ProductId
            case "string":
                return GenerateRandomString();
            case "bool":
                return random.Next(2) == 0;
            case "double":
            case "decimal":
                return Math.Round(random.NextDouble() * 1000, 2);  // More reasonable range for prices
            case "datetime":
                return DateTime.Now.AddDays(random.Next(-365, 365));
            case "dateonly":
                return DateOnly.FromDateTime(DateTime.Now.AddDays(random.Next(-365, 365)));
            case "datetimeoffset":
                return DateTimeOffset.Now.AddDays(random.Next(-365, 365));
            case "timeonly":
                return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(random.Next(0, 1440)));
            default:
                return GenerateRandomCustomType(property.Type, trackedClasses);
        }
    }

    private static bool IsEnum(string typeName, List<ClassDefinition> trackedClasses)
    {
        var customType = trackedClasses.FirstOrDefault(c => c.Name == typeName);
        return customType != null && customType.Properties.Any(p => p.Type.ToLower() == "enum");
    }

    private static object GenerateRandomEnum(PropertyDefinition property, List<ClassDefinition> trackedClasses)
    {
        var customType = trackedClasses.FirstOrDefault(c => c.Name == property.Type);
        if (customType != null && customType.Properties.Any())
        {
            var enumProperty = customType.Properties.First();
            if (enumProperty.Values != null && enumProperty.Values.Any())
            {
                return enumProperty.Values[random.Next(enumProperty.Values.Count)];
            }
        }
        return null;
    }


    public static void GenerateData(ClassDefinition classDef, List<ClassDefinition> trackedClasses, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var generatedObject = GenerateRandomCustomType(classDef.Name, trackedClasses);

            Console.WriteLine($"\nGenerated {classDef.Name} #{i + 1}:");
            Console.WriteLine("JSON Format:");
            Console.WriteLine(GenerateJsonOutput(generatedObject));

            Console.WriteLine("\nC# Object Initialization Format:");
            Console.WriteLine(GenerateCSharpOutput(classDef.Name, generatedObject,trackedClasses));
        }
    }
    private static string GenerateJsonOutput(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string GenerateCSharpOutput(string className, object obj, List<ClassDefinition> trackedClasses, string indent = "")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{indent}{className} {className.ToLower()}1 = new {className}()");
        sb.AppendLine($"{indent}{{");

        if (obj is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                var propertyType = GetPropertyType(className, kvp.Key, trackedClasses);

                if (IsEnum(propertyType, trackedClasses))
                {
                    sb.AppendLine($"{indent}    {kvp.Key} = {propertyType}.{kvp.Value},");
                }
                else if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    sb.AppendLine($"{indent}    {kvp.Key} = new {propertyType}()");
                    sb.AppendLine($"{indent}    {{");
                    foreach (var nestedKvp in nestedDict)
                    {
                        sb.AppendLine($"{indent}        {nestedKvp.Key} = {FormatValue(nestedKvp.Value, GetPropertyType(propertyType, nestedKvp.Key, trackedClasses), trackedClasses)},");
                    }
                    sb.AppendLine($"{indent}    }},");
                }
                else if (kvp.Value is List<object> list)
                {
                    var innerType = ExtractInnerType(propertyType);

                    sb.AppendLine($"{indent}    {kvp.Key} = new List<{innerType}>()");
                    sb.AppendLine($"{indent}    {{");
                    foreach (var item in list)
                    {
                        if (item is Dictionary<string, object> listItemDict)
                        {
                            sb.AppendLine($"{indent}        new {innerType}()");
                            sb.AppendLine($"{indent}        {{");
                            foreach (var listItemKvp in listItemDict)
                            {
                                sb.AppendLine($"{indent}            {listItemKvp.Key} = {FormatValue(listItemKvp.Value, GetPropertyType(innerType, listItemKvp.Key, trackedClasses), trackedClasses)},");
                            }
                            sb.AppendLine($"{indent}        }},");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}        {FormatValue(item, innerType, trackedClasses)},");
                        }
                    }
                    sb.AppendLine($"{indent}    }},");
                }
                else
                {
                    sb.AppendLine($"{indent}    {kvp.Key} = {FormatValue(kvp.Value, propertyType, trackedClasses)},");
                }
            }
        }

        sb.AppendLine($"{indent}}};");
        return sb.ToString();
    }
    private static string GetPropertyType(string className, string propertyName, List<ClassDefinition> trackedClasses)
    {
        var classDefinition = trackedClasses.FirstOrDefault(c => c.Name == className);
        if (classDefinition != null)
        {
            var property = classDefinition.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property != null)
            {
                return property.Type;
            }
        }
        return "object"; // Default to object if type is not found
    }

    private static string GetCustomTypeName(string typeName, List<ClassDefinition> trackedClasses)
    {
        // Handle generic types
        if (typeName.StartsWith("List<") || typeName.StartsWith("IEnumerable<"))
        {
            var innerType = ExtractInnerType(typeName);
            var customType = trackedClasses.FirstOrDefault(c => c.Name == innerType);
            return customType != null ? customType.Name : "object";
        }

        // Handle non-generic types
        var type = trackedClasses.FirstOrDefault(c => c.Name == typeName);
        return type != null ? type.Name : "object";
    }


    private static string ExtractInnerType(string type)
    {
        if (type.StartsWith("List<") || type.StartsWith("IEnumerable<"))
        {
            return type.Substring(type.IndexOf('<') + 1, type.LastIndexOf('>') - type.IndexOf('<') - 1);
        }
        return type;
    }


    private static string FormatValue(object value, string type, List<ClassDefinition> trackedClasses)
    {
        if (IsEnum(type, trackedClasses))
            return $"{type}.{value}";
        else if (value is string)
            return $"\"{value}\"";
        else if (value is DateTime dateTime)
            return $"DateTime.Parse(\"{dateTime:yyyy-MM-dd HH:mm:ss}\")";
        else if (value is DateOnly dateOnly)
            return $"DateOnly.Parse(\"{dateOnly:yyyy-MM-dd}\")";
        else if (value is TimeOnly timeOnly)
            return $"TimeOnly.Parse(\"{timeOnly:HH:mm:ss}\")";
        else if (value is bool)
            return value.ToString().ToLower();
        else
            return value.ToString();
    }

    private static void PrintObject(object obj, string indent = "")
    {
        if (obj is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value is Dictionary<string, object> || kvp.Value is List<object>)
                {
                    Console.WriteLine($"{indent}{kvp.Key}:");
                    PrintObject(kvp.Value, indent + "  ");
                }
                else
                {
                    Console.WriteLine($"{indent}{kvp.Key}: {kvp.Value}");
                }
            }
        }
        else if (obj is List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"{indent}[{i}]:");
                PrintObject(list[i], indent + "  ");
            }
        }
        else
        {
            Console.WriteLine($"{indent}{obj}");
        }
    }


    private static object GenerateRandomList(PropertyDefinition property, List<ClassDefinition> trackedClasses)
    {
        var innerType = ExtractInnerType(property.Type);
        int count = random.Next(1, 4);  // Generate a list with 1 to 3 items
        var list = new List<object>();
        for (int i = 0; i < count; i++)
        {
            list.Add(GenerateRandomValue(new PropertyDefinition("item", innerType), trackedClasses));
        }
        return list;
    }

  

    private static object GenerateRandomCustomType(string typeName, List<ClassDefinition> trackedClasses)
    {
        var customType = trackedClasses.FirstOrDefault(c => c.Name == typeName);
        if (customType == null)
        {
            throw new ArgumentException($"Unknown type: {typeName}");
        }

        var result = new Dictionary<string, object>();
        foreach (var property in customType.Properties)
        {
            result[property.Name] = GenerateRandomValue(property, trackedClasses);
        }
        return result;
    }


    private static string GenerateRandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}