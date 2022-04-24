using System.Diagnostics;
using System.Security.Principal;

namespace Myvas.Tools.IpFix;

public static class RunAsHelper
{
    /// <summary>
    /// Asks for administrator privileges upgrade if the platform supports it, otherwise does nothing
    /// </summary>
    /// <exception cref="ApplicationException">Unable to determine administrator or root status</exception>
    public static bool IsRunAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Unable to determine administrator or root status", ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ApplicationException">Unable to restart "..."</exception>
    private static void Restart()
    {
        var exe = Environment.ProcessPath!;
        var startInfo = new ProcessStartInfo(exe)
        {
            Verb = "runas",
            Arguments = "restart"
        };
        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Unable to restart \"{exe}\"", ex);
        }
    }
}

