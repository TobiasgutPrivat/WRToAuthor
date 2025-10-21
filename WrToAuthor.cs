using System.Text.Json;
using GBX.NET;
using GBX.NET.Engines.Game;

class WRtoAuthor
{
    public static void setWRAuthor(string path)
    {
        Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(path);
        CGameCtnChallenge map = gbx.Node;
        string mapUid = map.MapInfo.Id;

        JsonElement wr = API.getWR(mapUid).GetAwaiter().GetResult() ?? throw new Exception("No WR found");
        string accountId = wr.GetProperty("accountId").GetString();
        int score = wr.GetProperty("score").GetInt32();

        map.AuthorLogin = accountId;
        map.AuthorTime = new TmEssentials.TimeInt32(score);
        map.AuthorScore = score;
        gbx.Save(path + "modified.map.Gbx");

    }
}