using Microsoft.AspNetCore.Components;
using Microsoft.Web.Administration;
using ServiceManagement.Services;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    protected List<Server> servers = new List<Server>();
    [Inject] protected PreloadService PreloadService { get; set; } = null!;
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Inject] protected IRemoteIISManager RemoteIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;

    protected async Task StartService(string serverName, Service service, string? startupArguments)
    {
        await PreloadService.ShowWaitingService();

        ServiceManager.StartServiceAsync(serverName, service.Name, startupArguments);

        while (ServiceManager.GetServiceStatusAsync(serverName, service.Name) != ServiceControllerStatus.Running)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Running;

        await PreloadService.HideWaitingService();
    }

    protected async Task StopService(string serverName, Service service)
    {
        await PreloadService.ShowWaitingService();

        ServiceManager.StopServiceAsync(serverName, service.Name);

        while (ServiceManager.GetServiceStatusAsync(serverName, service.Name) != ServiceControllerStatus.Stopped)
            await Task.Delay(1000);

        service.Status = ServiceControllerStatus.Stopped;

        await PreloadService.HideWaitingService();
    }

    protected async Task StartAppPool(Server server, AppPool appPool)
    {
        await PreloadService.ShowWaitingAppPool();

        if (server.Location == ServerLocationType.Remote)
        {
            RemoteIISManager.StartAppPoolAsync(server.Name, appPool);

            while (RemoteIISManager.GetAppPoolStatusAsync(server.Name, appPool) != ObjectState.Started)
                await Task.Delay(1000);
        }
        else
        {
            LocalIISManager.StartAppPoolAsync(appPool);

            while (LocalIISManager.GetAppPoolStatusAsync(appPool) != ObjectState.Started)
                await Task.Delay(1000);
        }

        appPool.State = ObjectState.Started;

        await PreloadService.HideWaitingAppPool();
    }

    protected async Task StopAppPool(Server server, AppPool appPool)
    {
        await PreloadService.ShowWaitingAppPool();

        if (server.Location == ServerLocationType.Remote)
        {
            RemoteIISManager.StopAppPoolAsync(server.Name, appPool);

            while (RemoteIISManager.GetAppPoolStatusAsync(server.Name, appPool) != ObjectState.Stopped)
                await Task.Delay(1000);
        }
        else
        {
            LocalIISManager.StopAppPoolAsync(appPool);

            while (LocalIISManager.GetAppPoolStatusAsync(appPool) != ObjectState.Stopped)
                await Task.Delay(1000);
        }

        appPool.State = ObjectState.Stopped;

        await PreloadService.HideWaitingAppPool();
    }

    protected ServiceControllerStatus GetServiceState(string serverName, Service service) =>
        ServiceManager.GetServiceStatusAsync(serverName, service.Name);

    protected ObjectState GetAppPoolState(Server server, AppPool appPool)
    {
        if (server.Location == ServerLocationType.Remote)
            return RemoteIISManager.GetAppPoolStatusAsync(server.Name, appPool);
        else
            return LocalIISManager.GetAppPoolStatusAsync(appPool);
    }
}
