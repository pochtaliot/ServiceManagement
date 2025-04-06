using Microsoft.AspNetCore.Components;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.Partials.Services;

public class ServiceComponentClass : ComponentBase
{
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Parameter] public Server Server { get; set; } = null!;
    protected bool initialization { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            initialization = true;
            StateHasChanged();
            await RefreshServices();
            initialization = false;
            StateHasChanged();
        }
    }

    protected async Task RefreshServices()
    {
        await Task.Yield();

        foreach (var service in Server.Services)
        {
            service.IsInChangeState = true;
            StateHasChanged();
            service.Status = ServiceManager.GetServiceStatus(Server.Name, service.Name);
            service.IsInChangeState = false;
            StateHasChanged();
        }
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
