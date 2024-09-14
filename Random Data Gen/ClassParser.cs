using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class  ClassParser
{
    public static (string Name, string Type, List<(string Name, string Type)> Properties) ParseType(string typeContent)
    {
        typeContent = RemoveComments(typeContent);
        typeContent = Regex.Replace(typeContent, @"\s+", " ");

        if (typeContent.Contains("enum"))
        {
            return ParseEnum(typeContent);
        }
        else if (typeContent.Contains("record"))
        {
            return ParseRecord(typeContent);
        }
        else
        {
            return ParseClass(typeContent);
        }
    }

    private static (string Name, string Type, List<(string Name, string Type)> Properties) ParseClass(string classContent)
    {
        var classMatch = Regex.Match(classContent, @"public\s+(class|struct)\s+(\w+)");
        if (!classMatch.Success)
        {
            throw new ArgumentException("No valid class or struct definition found in the input.");
        }
        string className = classMatch.Groups[2].Value;

        var propertyMatches = Regex.Matches(classContent, @"public\s+(\w+(?:<[\w\s,]+>)?)\s+(\w+)\s*{\s*get;\s*set;\s*}");
        var properties = new List<(string Name, string Type)>();

        foreach (Match match in propertyMatches)
        {
            string type = match.Groups[1].Value.Trim();
            string name = match.Groups[2].Value.Trim();
            properties.Add((name, type));
        }

        return (className, "class", properties);
    }

    private static (string Name, string Type, List<(string Name, string Type)> Properties) ParseEnum(string enumContent)
    {
        var enumMatch = Regex.Match(enumContent, @"public\s+enum\s+(\w+)");
        if (!enumMatch.Success)
        {
            throw new ArgumentException("No valid enum definition found in the input.");
        }
        string enumName = enumMatch.Groups[1].Value;

        var valueMatches = Regex.Matches(enumContent, @"{\s*((?:\w+\s*(?:=\s*\d+)?\s*,?\s*)+)}");
        var values = new List<(string Name, string Type)>();

        if (valueMatches.Count > 0)
        {
            string[] enumValues = valueMatches[0].Groups[1].Value.Split(',');
            foreach (var value in enumValues)
            {
                var parts = value.Trim().Split('=');
                string name = parts[0].Trim();
                string valueStr = parts.Length > 1 ? parts[1].Trim() : "";
                values.Add((name, valueStr));
            }
        }

        return (enumName, "enum", values);
    }

    private static (string Name, string Type, List<(string Name, string Type)> Properties) ParseRecord(string recordContent)
    {
        var recordMatch = Regex.Match(recordContent, @"public\s+record\s+(\w+)");
        if (!recordMatch.Success)
        {
            throw new ArgumentException("No valid record definition found in the input.");
        }
        string recordName = recordMatch.Groups[1].Value;

        var propertyMatches = Regex.Matches(recordContent, @"(\w+(?:<[\w\s,]+>)?)\s+(\w+)(?:\s*[,)]|$)");
        var properties = new List<(string Name, string Type)>();

        foreach (Match match in propertyMatches)
        {
            string type = match.Groups[1].Value.Trim();
            string name = match.Groups[2].Value.Trim();
            properties.Add((name, type));
        }

        return (recordName, "record", properties);
    }

    private static string RemoveComments(string code)
    {
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
        code = Regex.Replace(code, @"//.*", "");
        return code;
    }
}
