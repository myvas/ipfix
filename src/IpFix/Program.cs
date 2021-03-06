using System.Reflection;

namespace Myvas.Tools.IpFix;

public static class Program
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"><code>ipfix github.com</code></param>
    static void Main(string[] args)
    {
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

            foreach (var dns in args)
            {
                if (!dns.StartsWith('-') && !dns.StartsWith('/')) Fix(dns);
            }

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

    public static void Fix(string dns)
    {
        try
        {
            var ipAddress = IpAddressFetcher.RetrieveIpAddress(dns);
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                Console.WriteLine($"No record of '{dns}' be found on {IpAddressFetcher.Name}.");
                return;
            }
            Console.WriteLine($"The IP address of {dns} is {ipAddress}");

            var hostsFile = new HostsFile();
            var changed = hostsFile.UpdateHostsRecord(dns, ipAddress, IpAddressFetcher.Name);
            if (!changed)
            {
                Console.WriteLine($"The record does not need to update.");
                return;
            }

            hostsFile.Write();
            Console.WriteLine($"{dns} updated to {ipAddress}");

            IpConfigHelper.Flushdns();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

