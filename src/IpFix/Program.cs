using System.Reflection;

namespace Myvas.Tools.IpFix;

public static class Program
{
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
        finally
        {
        }
    }
    public static async Task FixAsync(string dns)
    {
        IFetcher fetcher = new IpAddressFetcher();
        string ipAddress = "";
        try
        {
            Console.WriteLine($"Retrieving records via {fetcher.Name}...");
            ipAddress = await fetcher.RetrieveIpv4Async(dns);
        }
        catch
        {
            Console.WriteLine($"Failed while retrieving record for {dns} on {fetcher.Name}.");
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
                Console.WriteLine($"Failed while retrieving record for {dns} on {fetcher.Name}.");
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
            Console.WriteLine($"{dns} newly updated to {ipAddress}");

            IpConfigHelper.Flushdns();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

