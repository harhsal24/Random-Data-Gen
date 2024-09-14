


public static class FileHandler
{
    public static void AppendToFile(string content)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserClass.txt");
        Console.WriteLine($"File path: {filePath}");

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
            Console.WriteLine($"Error writing to file: {ex.Message}");
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
