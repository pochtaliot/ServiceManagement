using Microsoft.Web.Administration;
using System.ServiceProcess;

namespace ServiceManagement;

public class ServiceConfig
{
    public List<Server> Servers { get; set; } = new();
    public void SetAllToChangingState()
    {
        Servers.ForEach(f => f.Services.ForEach(ff => ff.IsInChangeState = true));
        Servers.ForEach(f => f.AppPools.ForEach(ff => ff.IsInChangeState = true));
    }
}

public class Server
{
    public string Name { get; set; } = "";
    public string Alias { get; set; } = "";
    public ServerLocationType Location { get; set; }
    public List<Service> Services { get; set; } = new();
    public List<AppPool> AppPools { get; set; } = new();
    public bool ShowStartupArguments { get; set; } = false;
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
    public bool IsInChangeState { get; set; } = true;
    public bool IsRunning => State == ObjectState.Started;
    public bool StateRetrievedSuccessfully { get; set; } = true;

}

public class Service
{
    public string Name { get; set; } = "";
    public ServiceControllerStatus Status { get; set; }
    public string? StartupArguments { get; set; }
    public bool IsInChangeState { get; set; } = true;
    public bool IsRunning => Status == ServiceControllerStatus.Running;
    public bool StateRetrievedSuccessfully { get; set; } = true;
    // Removed ShowStartupArguments, now handled at Server level
}