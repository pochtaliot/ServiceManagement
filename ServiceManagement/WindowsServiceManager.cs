using System.Management;
using System.ServiceProcess;

namespace ServiceManagement;

public interface IWindowsServiceManager
{
    void StartService(string serverName, string serviceName, string? startupArguments = null);
    void StopService(string serverName, string serviceName);
    ServiceControllerStatus GetServiceStatus(string serverName, string serviceName);
}

public class WindowsServiceManager : IWindowsServiceManager
{
    public void StartService(string serverName, string serviceName, string? startupArguments = null)
    {
        if (string.IsNullOrEmpty(startupArguments))
        {
            // Start without arguments using ServiceController
            using var sc = new ServiceController(serviceName, serverName);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
        }
        else
        {
            // Start with arguments using WMI
            var scope = new ManagementScope($"\\\\{serverName}\\root\\cimv2");
            scope.Connect();

            var query = new SelectQuery($"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'");
            using var searcher = new ManagementObjectSearcher(scope, query);

            foreach (ManagementObject service in searcher.Get())
            {
                var result = service.InvokeMethod("StartService", new object[] { startupArguments });
             
                if (result != null && (uint)result != 0)
                    throw new InvalidOperationException($"Failed to start service '{serviceName}' with arguments. Error code: {(uint)result}");
            }
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
}
