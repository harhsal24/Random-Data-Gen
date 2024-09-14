public static class Configuration
{
    public static bool TreatAsString =false;
    public static  Dictionary<string, string> DefaultFormats = new Dictionary<string, string>
    {
        { "DateTime", "yyyy-MM-dd HH:mm:ss" },
        { "DateOnly", "yyyy-MM-dd" },
        { "TimeOnly", "HH:mm:ss" },
        { "DateTimeOffset", "yyyy-MM-dd HH:mm:ss zzz" },
        { "TimeSpan", "hh:mm:ss" }
    };

    public static void SetCustomFormat(string type, string format)
    {
        // Implementation of setting custom format for special date/time types
    }


    //public static  Dictionary<string, bool> TreatAsString = new Dictionary<string, bool>
    //{
    //    { "DateTime", false },
    //    { "DateOnly", false },
    //    { "TimeOnly", false },
    //    { "DateTimeOffset", false },
    //    { "TimeSpan", false }
    //};

    //public static  Dictionary<string, bool> isFirstTime= new Dictionary<string, bool>
    //{
    //    { "DateTime", true },
    //    { "DateOnly", true},
    //    { "TimeOnly", true },
    //    { "DateTimeOffset", true},
    //    { "TimeSpan", true}
    //};
}
