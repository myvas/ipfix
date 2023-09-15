[中文](./README.md) | English

# ipfix
A command-line utility to update DNS records in the `hosts` file with the IP address from the "Outernet".

## Usage  
<pre style="background-color:black;color:white;">
C:\>ipfix
ipfix 6.0.4

Usage: ipfix [Option] [domain-name-1] [domain-name-2] ... [domain-name-n]

[Option]
  -q|--quiet    Run in quiet mode.

Description:
Step 1: Fetch the IP address from a resolver on the outernet;
Step 2: Add or update the record in the `hosts` file
Step 3: Run `ipconfig /flushdns`

Examples:
  "ipfix github.com" - will update the IP address of github.com in the `hosts` file.
  "ipfix github.com www.github.com" - will update the IP address of github.com and www.github.com in the `hosts` file.

Press any key to exit...
</pre>
The command above `ipfix` without any args will print the help information.

---

<pre style="background-color:black;color:white;">
C:\>ipfix github.com
ipfix 6.0.4

The IP address of github.com is 140.82.112.3
github.com updated to 140.82.112.3

Windows IP Configuration

Successfully flushed the DNS Resolver Cache.

Press any key to exit...
</pre>
Use `ipfix github.com` to update the following kind-of record in the `hosts` file:  
*C:\Windows\system32\drivers\etc\hosts*
```
140.82.112.3 www.github.com
```

## Program Flow
- Step 1  : Fetch the record of {domain-name} from an IP address resolver on the outernet;");
- Step 1.1: Resolve via [Best free & Public DNS Servers](https://www.lifewire.com/free-and-public-dns-servers-2626062)
    
    |Provider|Primary DNS|Secondary DNS|
    |-|-|-|
    |Google|	8.8.8.8|	8.8.4.4|
    |Control D|	76.76.2.0|	76.76.10.0|
    |Quad9|	9.9.9.9|	149.112.112.112|
    |OpenDNS Home|	208.67.222.222|	208.67.220.220|
    |Cloudflare|	1.1.1.1|	1.0.0.1|
    |CleanBrowsing|	185.228.168.9|	185.228.169.9|
    |Alternate DNS|	76.76.19.19|	76.223.122.150|
    |AdGuard DNS|	94.140.14.14| 94.140.15.15|
    
    NOTE: We try the primary DNS only, not secondary DNS; we will try the next provider on failure.
  
- Step 1.2: Query from [ipaddress.com](https://ipaddress.com/website/github.com).

- Step 1.3: Query from [nslookup.io](https://www.nslookup.io/domains/github.com/dns-records/#usa).

- Step 2.1: Add the new record of {domain-name} in the *hosts* file, ");
- Step 2.2: Or replace the old record with new one.

## Task Scheduler (Optional)
[Help for task scheduler](https://community.spiceworks.com/how_to/17736-run-powershell-scripts-from-task-scheduler)
- General
    - Name: myvas-fix-github-ipaddress
    - Description: Timed updating the hosts record of github.com
- Triggers
    - Every N minutes
- Actions
    - Start a program
    - Program/Script: `ipfix.exe`
    - Argument: `github.com www.github.com`
