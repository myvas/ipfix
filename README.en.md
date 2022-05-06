[中文](./README.md) | English

# ipfix
A tool to retrive IP address from [ipaddress.com](https://www.ipaddress.com), and save the record in the `hosts` file.

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
140.82.114.4 www.github.com
```

## Program Flow
- Step 1  : Fetch the record of {domain-name} from an IP address resolver on the outernet;");
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
