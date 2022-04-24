using System.Diagnostics;

namespace Myvas.Tools.IpFix;

public static class IpConfigHelper
{
    public static void Flushdns()
    {
        var exe = "ipconfig";
        var startInfo = new ProcessStartInfo(exe)
        {
            Arguments = "/flushdns"
        };
        try
        {
            Process.Start(startInfo)?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

