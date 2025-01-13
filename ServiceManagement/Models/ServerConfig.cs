using Microsoft.Web.Administration;
using System.ServiceProcess;

namespace ServiceManagement;

public class ServiceConfig
{
    public List<Server> Servers { get; set; } = new();
}

public class Server
{
    public string Name { get; set; } = "";
    public string Alias { get; set; } = "";
    public ServerLocationType Location { get; set; }
    public List<Service> Services { get; set; } = new();
    public List<AppPool> AppPools { get; set; } = new();
}

public enum ServerLocationType
{
    Local,
    Remote
}

public class AppPool
{
    public string Name { get; set; } = "";
    public ObjectState State { get; set; }
    public bool IsRunning => State == ObjectState.Started;

}

public class Service
{
    public string Name { get; set; } = "";
    public ServiceControllerStatus Status { get; set; }
    public string? StartupArguments { get; set; }
    public bool IsRunning => Status == ServiceControllerStatus.Running;
}