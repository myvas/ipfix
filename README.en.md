[中文](./README.md) | English

# ipfix
A tool to retrive IP address from [ipaddress.com](https://www.ipaddress.com), and save the record in the `hosts` file.

## Usage
![Alt](docs/screenshots/screenshot.png "ipfix github.com")

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
