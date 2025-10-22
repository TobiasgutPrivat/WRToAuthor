using GBX.NET;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();
// Gbx.ZLib = new ZLib();

string? path = "C:/Users/Tobias/Documents/Trackmania2020/Maps/AutoAlt/Altered Nadeo/Fall 2025/Fall 2025 Fragile"; // set to null for command line .exe
if (path == null)
{
    if (args.Length > 0)
    {
        path = args[0];
        Console.WriteLine("This will overwrite and upload directly, continue? (Y/N)");
        bool overwrite = Console.ReadKey().Key == ConsoleKey.Y;
        if (!overwrite)
        {
            Console.WriteLine("/nOperation cancelled.");
            Environment.Exit(0);
        }
    }
    else
    {
        Console.WriteLine("No file or folder specified");
        Environment.Exit(1);
    }
}


Console.WriteLine("Enter email: ");
string email = Console.ReadLine() ?? "";
Console.WriteLine("Enter password: ");
string password = Console.ReadLine() ?? "";
WRtoAuthor wRtoAuthor = new WRtoAuthor(email, password);
if (Directory.Exists(path))
{
    foreach (string file in Directory.EnumerateFiles(path, "*.Map.Gbx"))
    {
        wRtoAuthor.setWRAuthor(file);
    }
}
else if (File.Exists(path))
{
    wRtoAuthor.setWRAuthor(path);
}
else
{
    Console.WriteLine("File or folder not found: " + path);
    Environment.Exit(1);
}

Console.WriteLine("Done...");
Console.ReadLine();