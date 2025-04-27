using System.Management;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace ServiceManagement;

public interface IWindowsServiceManager
{
    void StartService(string serverName, string serviceName, string? startupArguments = null);
    void StopService(string serverName, string serviceName);
    ServiceControllerStatus GetServiceStatus(string serverName, string serviceName);
    string GetServiceStartupArguments(string serverName, string serviceName);
}

public class WindowsServiceManager : IWindowsServiceManager
{
    private readonly ManagementScopeDispatcher _scopeDispatcher;

    public WindowsServiceManager(ManagementScopeDispatcher scopeDispatcher)
    {
        _scopeDispatcher = scopeDispatcher;
    }

    public void StartService(string serverName, string serviceName, string? startupArguments = null)
    {
        using var sc = new ServiceController(serviceName, serverName);
        if (sc.Status != ServiceControllerStatus.Running)
        {
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
        }
    }

    public void StopService(string serverName, string serviceName)
    {
        using var sc = new ServiceController(serviceName, serverName);
        if (sc.Status != ServiceControllerStatus.Stopped)
        {
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        }
    }

    public ServiceControllerStatus GetServiceStatus(string serverName, string serviceName)
    {
        using var sc = new ServiceController(serviceName, serverName);
        return sc.Status;
    }

    public string GetServiceStartupArguments(string serverName, string serviceName)
    {
        var scope = _scopeDispatcher.GetScope(serverName);
        var query = new SelectQuery($"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'");
        using var searcher = new ManagementObjectSearcher(scope, query);

        foreach (ManagementObject service in searcher.Get())
        {
            var pathName = service.Properties["PathName"]?.Value?.ToString() ?? string.Empty;

            // Extract arguments after the executable path
            var match = Regex.Match(pathName, @"^(?:""[^""]+""|[^\s]+)(?:\s+(.*))?$");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return string.Empty; // No arguments found
        }

        throw new InvalidOperationException($"Service '{serviceName}' not found on server '{serverName}'.");
    }
}
