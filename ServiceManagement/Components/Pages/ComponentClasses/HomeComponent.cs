using Microsoft.AspNetCore.Components;
using Microsoft.Web.Administration;
using ServiceManagement.Services;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    [Inject] protected PreloadService PreloadService { get; set; } = null!;
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Inject] protected IPowershellIISManager PowershellIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;

    protected async Task StartService(string serverName, Service service, string? startupArguments)
    {
        service.IsInChangeState = true;
        await Task.Yield();

        ServiceManager.StartServiceAsync(serverName, service.Name, startupArguments);

        while (ServiceManager.GetServiceStatusAsync(serverName, service.Name) != ServiceControllerStatus.Running)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Running;

        service.IsInChangeState = false;
    }

    protected async Task StopService(string serverName, Service service)
    {
        service.IsInChangeState = true;
        await Task.Yield();

        ServiceManager.StopServiceAsync(serverName, service.Name);

        while (ServiceManager.GetServiceStatusAsync(serverName, service.Name) != ServiceControllerStatus.Stopped)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Stopped;

        service.IsInChangeState = false;
    }

    protected async Task StartAppPool(Server server, AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        if (server.Location == ServerLocationType.Remote)
        {
            PowershellIISManager.StartAppPoolAsync(server.Name, appPool);

            while (PowershellIISManager.GetAppPoolStatusAsync(server.Name, appPool) != ObjectState.Started)
                await Task.Delay(1000);
        }
        else
        {
            LocalIISManager.StartAppPoolAsync(appPool);

            while (LocalIISManager.GetAppPoolStatusAsync(appPool) != ObjectState.Started)
                await Task.Delay(1000);
        }

        appPool.State = ObjectState.Started;

        appPool.IsInChangeState = false;
    }

    protected async Task StopAppPool(Server server, AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        if (server.Location == ServerLocationType.Remote)
        {
            PowershellIISManager.StopAppPoolAsync(server.Name, appPool);

            while (PowershellIISManager.GetAppPoolStatusAsync(server.Name, appPool) != ObjectState.Stopped)
                await Task.Delay(1000);
        }
        else
        {
            LocalIISManager.StopAppPoolAsync(appPool);

            while (LocalIISManager.GetAppPoolStatusAsync(appPool) != ObjectState.Stopped)
                await Task.Delay(1000);
        }

        appPool.State = ObjectState.Stopped;

        appPool.IsInChangeState = false;
    }

    protected ServiceControllerStatus GetServiceState(string serverName, Service service) =>
        ServiceManager.GetServiceStatusAsync(serverName, service.Name);

    protected ObjectState GetAppPoolState(Server server, AppPool appPool)
    {
        if (server.Location == ServerLocationType.Remote)
            return PowershellIISManager.GetAppPoolStatusAsync(server.Name, appPool);
        else
            return LocalIISManager.GetAppPoolStatusAsync(appPool);
    }
}
