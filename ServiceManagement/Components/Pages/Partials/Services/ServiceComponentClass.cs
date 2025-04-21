using Microsoft.AspNetCore.Components;
using ServiceManagement.Components.Pages.ComponentClasses;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.Partials.Services;

public class ServiceComponentClass : ComponentBase
{
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Parameter] public IEnumerable<Server> Servers { get; set; } = Enumerable.Empty<Server>();
    [Parameter] public required InitializationState InitializationState { get; set; }
    [Parameter] public EventCallback<Server> OnRefreshClick { get; set; }

    protected async Task RefreshServices(Server server)
    {
        await OnRefreshClick.InvokeAsync(server);
    }

    protected async Task StartService(string serverName, Service service, string? startupArguments)
    {
        service.IsInChangeState = true;
        await Task.Yield();

        ServiceManager.StartService(serverName, service.Name, startupArguments);

        while (ServiceManager.GetServiceStatus(serverName, service.Name) != ServiceControllerStatus.Running)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Running;

        service.IsInChangeState = false;
    }

    protected async Task StopService(string serverName, Service service)
    {
        service.IsInChangeState = true;
        await Task.Yield();

        ServiceManager.StopService(serverName, service.Name);

        while (ServiceManager.GetServiceStatus(serverName, service.Name) != ServiceControllerStatus.Stopped)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Stopped;

        service.IsInChangeState = false;
    }
}
