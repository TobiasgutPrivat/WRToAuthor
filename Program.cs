using GBX.NET;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();
// Gbx.ZLib = new ZLib();

string? path = null; // set to null for command line .exe
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

if (Directory.Exists(path))
{
    foreach (string file in Directory.EnumerateFiles(path, "*.Map.Gbx"))
    {
        WRtoAuthor.setWRAuthor(file, email, password);
    }
}
else if (File.Exists(path))
{
    WRtoAuthor.setWRAuthor(path, email, password);
}
else
{
    Console.WriteLine("File or folder not found: " + path);
    Environment.Exit(1);
}

Console.WriteLine("Done...");
Console.ReadLine();