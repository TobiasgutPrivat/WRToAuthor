using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;
using TmEssentials;

class WRtoAuthor
{
    public static async void setWRAuthor(string path, string email, string password)
    {
        using var nls = new NadeoLiveServices();
        nls.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();
        using var ns = new NadeoServices();
        ns.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();

        Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(path);
        CGameCtnChallenge map = gbx.Node;
        string mapUid = map.MapInfo.Id;

        MapInfoLive mapInfo = nls.GetMapInfoAsync(mapUid).GetAwaiter().GetResult();
        Guid mapId = mapInfo.MapId;

        TopLeaderboardCollection leaderboard = nls.GetTopLeaderboardAsync(mapUid, 1).GetAwaiter().GetResult();
        Record wr = leaderboard.Tops.First().Top.First();
        map.AuthorLogin = Convert.ToBase64String(wr.AccountId.ToByteArray());
        map.AuthorTime = wr.Score;
        map.GoldTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.00106 + 1) * 1000);
        map.SilverTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0012 + 1) * 1000);
        map.BronzeTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0015 + 1) * 1000);
        map.AuthorScore = wr.Score.TotalMilliseconds;
        gbx.Save(path);
        Console.WriteLine($"{path} Author set to {wr.AccountId} {wr.Score}");

        using var fs = File.OpenRead(path);
        ns.UpdateMapAsync(mapId, fs, Path.GetFileName(fs.Name)).GetAwaiter().GetResult();
    }
    
}