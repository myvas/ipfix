using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Myvas.Tools.IpFix;

public class PingEuFetcher : IFetcher
{
    public string Name { get; } = "ping.eu";

    public async Task<string> RetrieveIpv4Async(string dns)
    {
        var result = "";

        var url = "https://ping.eu/action.php?atype=3";
        var data = new Dictionary<string, string>{
            {"go", "Go" },
            {"host", dns }
        };

        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync("https://ping.eu/nslookup/");
        Console.WriteLine(doc.Text);

        using var client = new HttpClient();
        using var response = await client.PostAsync(url, new FormUrlEncodedContent(data));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Content: {content}");

        // Pattern 1:
        try
        {
            /*
            <script>
            RESULTS.innerHTML += "github.com has address <span class=t2>140.82.121.4</span><br>";
            </script>
            */
            // "(?<dns>[^"\s]+)\shas address.*[\s>]+(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})[<\s]+.*
            var s_pattern = "\"(?<dns>[^\"\\s]+)\\shas address.*[\\s>]+(?<ip>\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})[<\\s]+.*";
            var s_options = RegexOptions.None;
            var regex = new Regex(s_pattern, s_options, TimeSpan.FromSeconds(3));
            var groupNames = regex.GetGroupNames();
            var groupNumbers = regex.GetGroupNumbers();
            var match = regex.Match(content);
            if (match.Success)
            {
                for (var i = 0; match.Success; i++, match = match.NextMatch())
                {
                    //Console.WriteLine("Match[{0}] is {1}: {2}", i, match.GetType().Name, match.Value);
                    for (var j = 0; j < groupNumbers.Length; j++)
                    {
                        var number = groupNumbers[j];
                        var name = groupNames[j];
                        var group = match.Groups[number];
                        var value = group.Success ? group.Value : "--- FAILURE ---";
                        //Console.WriteLine("  Group[{0} or '{1}'] is {2}: {3}", number, name, group.GetType().Name, value);
                        //for (var k = 0; k < group.Captures.Count; k++)
                        //{
                        //    var capture = group.Captures[k];
                        //    Console.WriteLine("    Capture[{0}] is {1}: {2}", k, capture.GetType().Name, capture.Value);
                        //}
                        if (name == "ip")
                        {
                            result = value;
                        }
                    }
                }
            }
        }
        catch
        {
        }

        return result;
    }
}

