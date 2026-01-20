using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;
using TmEssentials;

class WRtoAuthor
{
    private NadeoLiveServices nls;
    private NadeoServices ns;
    private String unvalidatedMark = " (Unvalidated)";
    public WRtoAuthor(string email, string password)
    {
        nls = new NadeoLiveServices();
        nls.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();
        ns = new NadeoServices();
        ns.AuthorizeAsync(email, password, AuthorizationMethod.UbisoftAccount).GetAwaiter().GetResult();
    }
    public void setWRAuthor(string mapPath, string? AuthorLogin = null, Guid? prefferedAccount = null, bool skipIfValidated = true, bool upload = true, bool markUnvalidated = false)
    {
        //Load map
        Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(mapPath);
        CGameCtnChallenge map = gbx.Node;
        TimeInt32 maxTime = new TimeInt32(1000000000); //estimate
        if (skipIfValidated && map.AuthorTime != null && map.AuthorTime < maxTime ) return; //already validated
        string mapUid = map.MapInfo.Id;

        //Get map info
        MapInfoLive mapInfo = nls.GetMapInfoAsync(mapUid).GetAwaiter().GetResult();
        Guid mapId = mapInfo.MapId;

        //Get WR
        TopLeaderboardCollection leaderboard = nls.GetTopLeaderboardAsync(mapUid, 1).GetAwaiter().GetResult();
        if (leaderboard.Tops.Count == 0 || leaderboard.Tops.First().Top.Count == 0)
        {
            Console.WriteLine($"{mapPath} has no WR");
            if (!markUnvalidated) return;
            Console.WriteLine($"{mapPath} marked as unvalidated");
            map.MapName += " (Unvalidated)";
            map.AuthorTime = maxTime;
            map.GoldTime = maxTime;
            map.SilverTime = maxTime;
            map.BronzeTime = maxTime;
            gbx.Save(mapPath += " (Unvalidated)");
        } else {
            List<Record> wrs = leaderboard.Tops.First().Top.ToList();
            Record? wr = null;
            if (prefferedAccount != null) { 
                //prefer specified author
                wr = wrs.FirstOrDefault(r => r.AccountId == prefferedAccount);   
            }
            if (wr == null){
                wr = wrs.First();
            }
            
            //Download Replay
            var records = ns.GetMapRecordsAsync([wr.AccountId], mapId).GetAwaiter().GetResult();
            MapRecord wrRec = records.First();
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
            if (AuthorLogin != null)
            {
                map.AuthorLogin = AuthorLogin;
                if (AuthorLogin != replay.GhostLogin)
                {
                    map.MapName += $" (ft. {replay.GhostNickname})";
                }
            } else
            {
                map.AuthorLogin = replay.GhostLogin;
            }
            map.AuthorTime = wr.Score;
            map.GoldTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.00106 + 1) * 1000);
            map.SilverTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0012 + 1) * 1000);
            map.BronzeTime = new TimeInt32((int)Math.Floor(wr.Score.TotalMilliseconds * 0.0015 + 1) * 1000);
            map.AuthorScore = wr.Score.TotalMilliseconds;
            
            //Cleanup
            File.Delete(replayPath);
            Console.WriteLine($"{mapPath} Author set to {wr.AccountId} {wr.Score}");
            map.MapName = map.MapName.Replace(unvalidatedMark, "");
            gbx.Save(mapPath.Replace(unvalidatedMark, ""));
        }

        if (!upload) return;
        using var mapfs = File.OpenRead(mapPath);
        ns.UpdateMapAsync(mapId, mapfs, Path.GetFileName(mapfs.Name)).GetAwaiter().GetResult();
        Console.WriteLine($"{mapPath} uploaded to Nadeo Servers");
    }
    
}