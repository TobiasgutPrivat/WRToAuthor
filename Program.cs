using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using System.Text.Json;

Gbx.LZO = new MiniLZO();
// Gbx.ZLib = new ZLib();

string? path = "C:/Users/tobia/Documents/Trackmania/Maps/Downloaded/CAN'T FIND A NAME, DEAL WITH IT.Map.Gbx";
if (path == null)
{
    if (args.Length > 0)
    {
        path = args[0];
        if (System.IO.Directory.Exists(path))
        {
            foreach (string file in System.IO.Directory.EnumerateFiles(path, "*.Map.Gbx"))
            {
                WRtoAuthor.setWRAuthor(file);
            }
        }
        else if (System.IO.File.Exists(path))
        {
            WRtoAuthor.setWRAuthor(path);
        }
        else
        {
            Console.WriteLine("File or folder not found: " + path);
            Environment.Exit(1);
        }
    }
    else
    {
        Console.WriteLine("No file or folder specified");
        Environment.Exit(1);
    }
}