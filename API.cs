using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

class API
{
    public static class Config
    {
        public static string login = "WRtoAuthor"; //accountId = ac22cd1d-3e32-4620-86b2-a35433f78c51
        public static string password = "c7*Ub0>U)X.^t@W)";
        public static string userAgent = "your_user_agent";
    }

    public static string getAccessToken()
    {
        var client = new HttpClient();
        var base64String = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{Config.login}:{Config.password}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic")
        {
            Content = new StringContent($"{{ \"audience\": \"NadeoLiveServices\" }}", Encoding.UTF8, "application/json"),
            Headers =
            {
                { "Authorization", $"Basic {base64String}" },
                { "User-Agent", Config.userAgent }
            }
        };
        var response = client.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            return body["accessToken"];
        }
        else
        {
            throw new Exception($"Failed to get access token: {response.StatusCode}");
        }
    }

    public static async Task<JsonElement?> getWR(string mapUid)
    {
        string url = $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/{"Personal_Best"}/map/{mapUid}/top?length={1}&onlyWorld={true}";
        string access_token = API.getAccessToken();
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={access_token}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(content);
            JsonElement firstRecord = doc.RootElement.GetProperty("tops").EnumerateArray().First().GetProperty("top").EnumerateArray().First();
            return firstRecord;
        }
        return null;
    }
}