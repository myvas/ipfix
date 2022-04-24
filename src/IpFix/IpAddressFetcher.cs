using HtmlAgilityPack;

namespace Myvas.Tools.IpFix;

public class IpAddressFetcher
{
    public const string Name = "ipaddress.com";

    public static string RetrieveIpAddress(string dns)
    {
        var result = "";

        var url = $"https://ipaddress.com/website/{dns}";

        var web = new HtmlWeb();
        var doc = web.Load(url);

        // Pattern 1: https://ipaddress.com/website/github.com
        try
        {
            /*
            <tr><th>IP Address</th><td><ul class="comma-separated"><li>140.82.114.3</li></ul></td></tr>
             */
            var node = doc.DocumentNode
                .SelectNodes("//tr[.//th[text()='IPv4 Address' or text()='IPv4 Addresses']]/td/ul/li")
                .FirstOrDefault();

            result = node?.InnerText;
        }
        catch
        {
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            // Then try Pattern 2: https://ipaddress.com/website/cdnjs.cloudflare.com
            try
            {
                /*
                <div>cdnjs.cloudflare.com resolves to 2 IPv4 addresses and 2 IPv6 addresses:<ul><li><strong>104.17.24.14</strong></li><li><strong>104.17.25.14</strong></li><li><strong>2606:4700::6811:180e</strong></li><li><strong>2606:4700::6811:190e</strong></li></ul></div>
                 */
                var node = doc.DocumentNode
                    .SelectNodes("//div[starts-with(text(),'" + dns + "')]/ul/li")
                    .FirstOrDefault();

                result = node?.InnerText;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Sorry, I cannot get a result. Please do me a favor to add an issue on https://github.com/myvas/ipfix, titled with 'Failed on {url}'.", ex);
            }
        }

        return result;
    }
}

