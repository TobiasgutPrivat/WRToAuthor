using System.Runtime.CompilerServices;
using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;
using TmEssentials;

class WRtoAuthor
{
    private NadeoLiveServices nls;
    private NadeoServices ns;
    public WRtoAuthor(string email, string password)
    {
        nls = new NadeoLiveServices();
        nls.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();
        ns = new NadeoServices();
        ns.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();
    }
    public void setWRAuthor(string mapPath)
    {
        //Load map
        Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(mapPath);
        CGameCtnChallenge map = gbx.Node;
        string mapUid = map.MapInfo.Id;

        //Get map info
        MapInfoLive mapInfo = nls.GetMapInfoAsync(mapUid).GetAwaiter().GetResult();
        Guid mapId = mapInfo.MapId;

        //Get WR
        TopLeaderboardCollection leaderboard = nls.GetTopLeaderboardAsync(mapUid, 1).GetAwaiter().GetResult();
        if (leaderboard.Tops.Count == 0 || leaderboard.Tops.First().Top.Count == 0)
        {
            Console.WriteLine($"{mapPath} has no WR data, skipping...");
            return;
        }
        Record wr = leaderboard.Tops.First().Top.First();
        
        //Download Replay
        MapRecord wrRec = ns.GetMapRecordsAsync([wr.AccountId], mapId).GetAwaiter().GetResult().First();
        string downloadURL = wrRec.Url;
        using var httpClient = new HttpClient();
        using var response = httpClient.GetAsync(downloadURL).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        string tempPath = Path.GetTempPath();
        string replayPath = Path.Combine(tempPath, $"wr_replay_{wr.AccountId}_{wr.Score.TotalMilliseconds}.gbxreplay");
        using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
        using var replayfs = new FileStream(replayPath, FileMode.Create);
        stream.CopyToAsync(replayfs).GetAwaiter().GetResult();
        replayfs.Close();
        
        //Load Replay
        Gbx<CGameCtnGhost> replayGbx = Gbx.Parse<CGameCtnGhost>(replayPath);;
        CGameCtnGhost replay = replayGbx.Node;

        //Set Author Data
        map.AuthorLogin = replay.GhostLogin;
        map.AuthorTime = wr.Score;
        map.GoldTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.00106 + 1) * 1000);
        map.SilverTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0012 + 1) * 1000);
        map.BronzeTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0015 + 1) * 1000);
        map.AuthorScore = wr.Score.TotalMilliseconds;
        gbx.Save(mapPath);
        Console.WriteLine($"{mapPath} Author set to {wr.AccountId} {wr.Score}");

        using var mapfs = File.OpenRead(mapPath);
        ns.UpdateMapAsync(mapId, mapfs, Path.GetFileName(mapfs.Name)).GetAwaiter().GetResult();

        //Cleanup
        File.Delete(replayPath);
    }
    
}