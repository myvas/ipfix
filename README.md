中文 | [English](README.en.md)

# ipfix
一个工具(控制台程序)：指定一个域名，程序将从外网获取IP地址，更新本机hosts文件，并刷新DNS缓存。非常地好用！

- 工作原理：本程序从*https://ipaddress.com*上查询IP地址。
- 应用1：国内直接`ping`到的IP地址经常无法正常访问国外网站，原因是`域名被劫持`，此时，您可能需要本程序。
- 应用2：通过本程序，您可以一次查询更新多个域名。
- 应用3：结合Windows任务计划程序，您可以定时执行查询更新。
- 不适用：本程序不适用于`IP地址被墙`造成的访问困难。


## 示例
![Alt](docs/screenshots/screenshot.png "ipfix github.com")

- `ipfix github.com`：将在hosts文件中更新(若无则新增)一条记录，如下：  
*C:\Windows\system32\drivers\etc\hosts*
```
140.82.114.4 github.com
```
注意：这个IP地址只是一个示例，事实上github.com的IP地址很频繁地在几个地址之间反复左右横跳；若您使用了不正确的IP地址，则很可能造成访问困难。



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
    - Program/Script: `Powershell.exe`
    - Argument: `-ExecutionPolicy Bypass C:\Windows\system32\drivers\etc\myvas-fix-github-ipaddress.ps1`
