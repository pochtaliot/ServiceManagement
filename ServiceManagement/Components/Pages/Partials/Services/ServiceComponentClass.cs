using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.Partials.Services;

public class ServiceComponentClass : ComponentBase
{
    [Inject] protected IOptionsSnapshot<ServiceConfig> Config { get; set; } = null!;
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    protected bool initialization { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            initialization = true;
            StateHasChanged();

            var refreshTasks = Config.Value.Servers
            .Select(server => RefreshServices(server))
            .ToList();

            await Task.WhenAll(refreshTasks);

            initialization = false;
            StateHasChanged();
        }
    }

    protected async Task RefreshServices(Server server)
    {
        foreach (var service in server.Services)
        {
            service.IsInChangeState = true;
            StateHasChanged();
            service.Status = ServiceManager.GetServiceStatus(server.Name, service.Name);
            service.IsInChangeState = false;
            StateHasChanged();
        }

        await Task.CompletedTask;
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
