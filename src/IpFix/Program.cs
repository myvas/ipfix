using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Myvas.Tools.IpFix;

public static class Program
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"><code>ipfix github.com</code></param>
    static void Main(string[] args)
    {
        try
        {
            if (!RunAsHelper.IsRunAsAdministrator())
            {
                Console.WriteLine($"This application must be run as administrator.");
                Console.WriteLine($"Please right click the {AppDomain.CurrentDomain.FriendlyName} file and select 'run as administrator'.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ipfix {domain-name-1} [domain-name-2] ...");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Step 1  : Fetch the record of {domain-name} from an IP address resolver on the outernet;");
                Console.WriteLine("Step 2.1: Add the new record of {domain-name} in the *hosts* file, ");
                Console.WriteLine("Step 2.2: Or replace the old record with new one.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  \"ipfix github.com\" - will update the IP address of github.com in the *hosts* file.");
                Console.WriteLine("  \"ipfix github.com www.github.com\" - will update the IP address of github.com and www.github.com in the *hosts* file.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            foreach (var dns in args)
            {
                if (!dns.StartsWith('-') && !dns.StartsWith('/')) Fix(dns);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            //try
            //{
            //    var isInConsole = Console.CursorLeft >= int.MinValue;
            //    Console.WriteLine();
            //    Console.WriteLine("Press any key to exit...");
            //    Console.ReadKey();
            //}
            //catch { }
        }
    }

    public static void Fix(string dns)
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
}

