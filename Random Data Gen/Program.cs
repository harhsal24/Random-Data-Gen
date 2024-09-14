using System;

class Program
{
    static void Main(string[] args)
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
    }
}
