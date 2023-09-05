using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace Myvas.Tools.IpFix;

public class NslookupExeFetcher : IFetcher
{
    public string Name { get; } = "NslookupExe";

    /// <summary>
    /// Extract the target IPv4 address from nslookup.exe stdout
    /// </summary>
    /// <param name="dns"></param>
    /// <param name="s_input">
    /// <code>
    /// Server:  dns.google
    /// Address:  8.8.8.8
    ///
    /// Non-authoritative answer:
    /// Name:    github.com
    /// Address:  20.205.243.166
    /// </code>
    /// </param>
    /// <returns></returns>
    private static string ExtractIpv4(string dns, string s_input)
    {
        var dnsInPattern = dns.Replace(".", "\\.");


        var s_pattern = "Name:\\s*" + dnsInPattern + "\\s*[\\n\\^\\s]+Address:\\s*([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)\\s*$";
        var s_options = RegexOptions.Multiline;
        var regex = new Regex(s_pattern, s_options, TimeSpan.FromMilliseconds(1000));

        var groupNames = regex.GetGroupNames();
        var groupNumbers = regex.GetGroupNumbers();
        var match = regex.Match(s_input);
        if (match.Success)
        {
            Debug.WriteLine("Input matches pattern");
            for (var i = 0; match.Success; i++, match = match.NextMatch())
            {
                Debug.WriteLine("Match[{0}] is {1}: {2}", i, match.GetType().Name, match.Value);

                for (var j = 0; j < groupNumbers.Length; j++)
                {
                    var number = groupNumbers[j];
                    var name = groupNames[j];
                    var group = match.Groups[number];
                    var value = group.Success ? group.Value : "--- FAILURE ---";
                    Debug.WriteLine("  Group[{0} or '{1}'] is {2}: {3}", number, name, group.GetType().Name, value);

                    for (var k = 0; k < group.Captures.Count; k++)
                    {
                        var capture = group.Captures[k];
                        Debug.WriteLine("    Capture[{0}] is {1}: {2}", k, capture.GetType().Name, capture.Value);
                        var t = capture.Value;
                        if (!t.StartsWith("Name"))
                            return t;
                    }
                }
            }
        }
        else
        {
            Debug.WriteLine("Input does not match pattern");
        }
        return "";
    }

    public string SelectedNameServer { get; set; } = "8.8.8.8";

    public override string ToString()
    {
        return $"{Name}@{SelectedNameServer}";
    }

    public async Task<string> RetrieveIpv4Async(string dns)
    {
        var result = "";

        try
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "nslookup.exe";
            psi.Arguments = $"{dns} {SelectedNameServer}";
            /// Redirect stdout to stream
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            psi.CreateNoWindow = true;
            p.StartInfo = psi;
            p.Start();

            p.WaitForExit();
            var output = await p.StandardOutput.ReadToEndAsync();
            result =ExtractIpv4(dns, output);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Sorry, I cannot get a result. Please do me a favor to add an issue on https://github.com/myvas/ipfix, titled with 'Failed on {Name}@{SelectedNameServer}'.", ex);
        }

        return result;
    }
}

