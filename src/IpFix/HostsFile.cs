using System.Text.RegularExpressions;

namespace Myvas.Tools.IpFix;

public class HostsFile
{
    public HostsFile()
    {
        var systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        FileNamePath = Path.Combine(systemFolder, "drivers", "etc", "hosts");

        Lines = File.ReadAllLines(FileNamePath);
    }

    public string FileNamePath { get; private set; }
    public string[] Lines { get; set; }

    public bool UpdateHostsRecord(string dns, string ipAddress, string providerName = IpAddressFetcher.Name)
    {
        var changed = false;
        var oldLine = Lines
            .Where(x => !x.StartsWith("#") && x.Contains(" " + dns))
            .FirstOrDefault();
        if (oldLine != null)
        {
            var newLine = ReplaceRecordLine(oldLine, dns, ipAddress);
            newLine ??= "";
            if (oldLine != newLine)
            {
                var position = Array.FindIndex(Lines, x => x == oldLine);
                Lines[position] = Lines[position].Replace(oldLine, newLine);
                changed = true;
            }
        }
        else
        {
            NewRecordLine(dns, ipAddress, providerName).ToList().ForEach(x => Lines = Lines.Append(x).ToArray());
            changed = true;
        }
        return changed;
    }

    public void Write()
    {
        // Backup the file
        File.Copy(FileNamePath, FileNamePath + DateTime.Now.ToString(".ddHHmm"), true);
        try
        {
            File.WriteAllLines(FileNamePath, Lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static string ReplaceRecordLine(string line, string dns, string ipAddress)
    {
        var pattern = "^.*[\\s]+" + dns;
        var regex = new Regex(pattern);
        var match = regex.Match(line);
        if (match.Success)
        {
            return $"{ipAddress} {dns}";
        }
        else
        {
            return line;
        }
    }

    public static string[] NewRecordLine(string dns, string ipAddress, string providerName)
    {
        var lines = new List<string>
        {
            $"# The following line was fetched from {providerName}. (https://github.com/myvas/ipfix)",
            $"{ipAddress} {dns}"
        };
        return lines.ToArray();
    }
}
