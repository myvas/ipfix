using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myvas.Tools.IpFix
{
    public interface IFetcher
    {
        string Name { get; }
        Task<string> RetrieveIpv4Async(string dns);
    }
}
