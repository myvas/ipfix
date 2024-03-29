using System.Diagnostics;
using System.Reflection;

namespace Myvas.Tools.IpFix;

public static class Program
{
    /// <summary>
    /// Best free public DNS servers: https://www.lifewire.com/free-and-public-dns-servers-2626062
    /// </summary>
    static string[] CandidateNameServers = new[] {
        "8.8.8.8", "76.76.2.0", "9.9.9.9", "208.67.222.222", "1.1.1.1",
        "185.228.168.9", "76.76.19.19", "94.140.14.14"
    };

    static List<string> ReadNameServersFromFile()
    {
        var result = new List<string>();
        var path = AppDomain.CurrentDomain.BaseDirectory;
        Debug.WriteLine(path);
        var filename = Path.Combine(path, "ipfix.data");
        try
        {
            using var sr = new StreamReader(filename);
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                result.Add(line);
            }
            sr.Close();
        }
        catch { }
        var count = result.Count;
        if (count < 2)
        {
            result = new List<string>(CandidateNameServers);
            count = result.Count;
        }
        try
        {
            var sw = new StreamWriter(filename);
            for (var i = 1; i < count; i++)
            {
                sw.WriteLine(result[i]);
            }
            sw.WriteLine(result[0]);
            sw.Close();
        }
        catch
        {
        }
        return result;
    }

    static async Task Main(string[] args)
    {
        // e.g. ipfix github.com
        var assembly = typeof(Program).Assembly;
        var assemblyName = assembly.GetName().Name;
        var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var title = $"{assemblyName} {assemblyVersion}";
        Console.WriteLine(title); Console.WriteLine();
        //Console.Title = title;

        try
        {
            if (!RunAsHelper.IsRunAsAdministrator())
            {
                Console.WriteLine($"This application must be run as administrator.");
                Console.WriteLine($"Please right click the {AppDomain.CurrentDomain.FriendlyName} file and select 'run as administrator'.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ipfix [Option] [domain-name-1] [domain-name-2] ... [domain-name-n]");
                Console.WriteLine();
                Console.WriteLine("[Option]");
                Console.WriteLine("  -q|--quiet    Run in quiet mode.");
                Console.WriteLine();
                Console.WriteLine("Description:");
                Console.WriteLine("Step 1: Fetch the IP address from a resolver on the outernet;");
                Console.WriteLine("Step 2: Add or update the record in the `hosts` file");
                Console.WriteLine("Step 3: Run `ipconfig /flushdns`");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  \"ipfix github.com\" - will update the IP address of github.com in the `hosts` file.");
                Console.WriteLine("  \"ipfix github.com www.github.com\" - will update the IP address of github.com and www.github.com in the `hosts` file.");
                Console.WriteLine();
                Console.WriteLine("Source code: https://github.com/myvas/ipfix.git");
                Console.WriteLine("License: MIT");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var quietMode = args.Contains("-q")
                || args.Contains("--quiet");
            if (quietMode)
            {
                args = args.Except(new[] { "-q", "--quiet" }).ToArray();
            }

            var allTasks = new List<Task>();
            foreach (var dns in args)
            {
                if (!dns.StartsWith('-') && !dns.StartsWith('/')) allTasks.Add(FixAsync(dns));
            }
            await Task.WhenAll(allTasks);

            if (!quietMode)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public static async Task FixAsync(string dns)
    {
        string ipAddress = "";
        IFetcher fetcher = new NslookupExeFetcher();

        var candidateNameServers = ReadNameServersFromFile();
        var count = candidateNameServers.Count;
        for (var i = 0; i < count; i++)
        {
            if (i != 0) candidateNameServers = ReadNameServersFromFile();
            try
            {
                ((NslookupExeFetcher)fetcher).SelectedNameServer = candidateNameServers[0];
                Console.WriteLine($"Retrieving records via {fetcher}...");
                ipAddress = await fetcher.RetrieveIpv4Async(dns);
                if (!string.IsNullOrWhiteSpace(ipAddress)) break;
            }
            catch
            {
                Console.WriteLine($"Failed while retrieving record for {dns} on {fetcher}.");
            }
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ((NslookupExeFetcher)fetcher).SelectedNameServer = "";
            try
            {
                Console.WriteLine($"Retrieving records via {fetcher.Name}...");
                ipAddress = await fetcher.RetrieveIpv4Async(dns);
            }
            catch
            {
                Console.WriteLine($"Failed while retrieving record for {dns} on {fetcher.Name}.");
            }
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            fetcher = new IpAddressFetcher();
            try
            {
                Console.WriteLine($"Retrieving records via {fetcher.Name}...");
                ipAddress = await fetcher.RetrieveIpv4Async(dns);
            }
            catch
            {
                Console.WriteLine($"Failed while retrieving record for {dns} on {fetcher.Name}.");
            }
        }
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            fetcher = new NslookupIoUsaFetcher();
            try
            {
                Console.WriteLine($"Retrieving records via {fetcher.Name}...");
                ipAddress = await fetcher.RetrieveIpv4Async(dns);
            }
            catch
            {
                Console.WriteLine($"Failed to retrieve record of {dns} on {fetcher.Name}.");
            }
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                Console.WriteLine($"No record of '{dns}' be found.");
                return;
            }
        }
        Console.WriteLine($"The IP address of {dns} is {ipAddress}");

        try
        {
            var hostsFile = new HostsFile();
            var changed = hostsFile.UpdateHostsRecord(dns, ipAddress, "Myvas.Tools.IpFix");
            if (!changed)
            {
                Console.WriteLine($"The hosts record is up to date.");
                return;
            }

            hostsFile.Write();
            Console.WriteLine($"{dns} is updated to {ipAddress}");

            IpConfigHelper.Flushdns();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

