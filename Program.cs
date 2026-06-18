using GBX.NET;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();
// Gbx.ZLib = new ZLib();

string? path = @"C:\Users\Tobias\Documents\Trackmania2020\Maps\Altered Process\Winter 2026\Winter 2026 WRTrace"; // set to null for command line .exe
string? login = "qfHOi30uQlySPIRmhdUeVw"; //Tobias2g
Guid? accountId = new Guid("a9f1ce8b-7d2e-425c-923c-846685d51e57"); //Tobias2g

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


string name = "service_AutoAlteration";
string password = "";
WRtoAuthor wRtoAuthor = new WRtoAuthor(name, password);
if (Directory.Exists(path))
{
    foreach (string file in Directory.EnumerateFiles(path, "*.Map.Gbx"))
    {
        wRtoAuthor.setWRAuthor(file,login,accountId);
    }
}
else if (File.Exists(path))
{
    wRtoAuthor.setWRAuthor(path,login,accountId);
}
else
{
    Console.WriteLine("File or folder not found: " + path);
    Environment.Exit(1);
}
#if !DEBUG
Environment.Exit(0);
#endif
if (args.Length > 0) { //command line mode
    Console.WriteLine("Done...");
    Console.ReadLine();
    Environment.Exit(0);
}