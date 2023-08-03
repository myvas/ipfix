using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Win32;
using PuppeteerSharp;

namespace Myvas.Tools.IpFix;

public class NslookupIoUsaFetcher : IFetcher
{
    public string Name { get; } = "nslookup.io";

    public async Task<string> RetrieveIpv4Async(string dns)
    {
        var result = "";

        var url = $"https://www.nslookup.io/domains/{dns}/dns-records/#usa";
        Debug.WriteLine(url);

        // Get chrome path from Windows registry
        const string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
        // var chromePath = (string)Registry.GetValue(keyName, "", "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe");
        var chromePath = (string)Registry.GetValue(keyName, "", null);
        Debug.WriteLine($"chrome path: {chromePath}");
        if (!File.Exists(chromePath))
        {
            Console.WriteLine("This app needs 'chrome.exe'. You should install the Google Chrome to continue...");
            return "";
        }
        var options = new LaunchOptions()
        {
            Headless = true,
            ExecutablePath = chromePath
        };

        using var browser = await Puppeteer.LaunchAsync(options);
        var page = await browser.NewPageAsync();
        await page.GoToAsync(url);

        var content = await page.GetContentAsync();
        Debug.WriteLine($"Content: {content}");

        // Pattern 1:
        try
        {
            /*
            <tr><th>IP Address</th><td><ul class="comma-separated"><li>140.82.114.3</li></ul></td></tr>
             */
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var node = doc.DocumentNode?
                .SelectNodes("//tr[@class='group']//td[@class='py-1']//span")
                .FirstOrDefault();

            Debug.WriteLine(node?.InnerText);
            result = node?.InnerText;
        }
        catch
        {
        }

        return result;
    }
}

