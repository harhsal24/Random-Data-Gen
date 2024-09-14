using System;
using System.Collections.Generic;
using System.Reflection;

class DataGenerator
{
    private static readonly Random random = new Random();

    public static object GenerateRandomValue(string type)
    {
        if (type.StartsWith("List<") || type.StartsWith("IEnumerable<"))
        {
            var innerType = ExtractInnerType(type);
            int count = random.Next(1, 6); // Generate a list with 1 to 5 items
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(Type.GetType(innerType))) as IList<object>;

            for (int i = 0; i < count; i++)
            {
                list.Add(GenerateRandomValue(innerType));
            }
            return list;
        }

        switch (type)
        {
            case "int":
                return random.Next();
            case "string":
                return GenerateRandomString();
            case "bool":
                return random.Next(2) == 0;
            case "double":
                return random.NextDouble();
            case "decimal":
                return (decimal)random.NextDouble();
            case "DateTime":
                return DateTime.Now.AddDays(random.Next(0, 365));
            case "DateOnly":
                return DateOnly.FromDateTime(DateTime.Now.AddDays(random.Next(0, 365)));
            case "DateTimeOffset":
                return DateTimeOffset.Now.AddDays(random.Next(0, 365));
            case "TimeOnly":
                return TimeOnly.FromTimeSpan(TimeSpan.FromHours(random.Next(0, 24)));
            default:
                throw new ArgumentException("Unsupported type");
        }
    }

    public static void GenerateData(List<FieldInfo> fields)
    {
        foreach (var field in fields)
        {
            object randomData = GenerateRandomValue(field.Type);
            Console.WriteLine($"{field.Name}: {randomData}");
        }
    }

    private static string GenerateRandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string ExtractInnerType(string type)
    {
        if (type.StartsWith("List<") || type.StartsWith("IEnumerable<"))
        {
            return type.Substring(type.IndexOf('<') + 1, type.IndexOf('>') - type.IndexOf('<') - 1);
        }
        return type;
    }
}
