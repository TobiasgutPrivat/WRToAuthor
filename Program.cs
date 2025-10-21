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
        if (!System.IO.File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            Environment.Exit(1);
        }
    }
    else
    {
        Console.WriteLine("No file specified");
        Environment.Exit(1);
    }
}

Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(path);
CGameCtnChallenge map = gbx.Node;
string mapUid = map.MapInfo.Id;

JsonElement wr = API.getWR(mapUid).GetAwaiter().GetResult() ?? throw new Exception("No WR found");
string accountId = wr.GetProperty("accountId").GetString();
int score = wr.GetProperty("score").GetInt32();

map.AuthorLogin = accountId;
map.AuthorNickname = "Test";
map.AuthorTime = new TmEssentials.TimeInt32(score);
map.AuthorScore = score;
gbx.Save(path + "modified.map.Gbx");
