using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
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
    [Inject] protected IOptionsSnapshot<ServiceConfig> Config { get; set; } = null!;
    protected bool initialization { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            initialization = true;
            StateHasChanged();

            foreach (var server in Config.Value.Servers)
            {
                await RefreshServices(server);
                await RefreshAppPools(server);
            }

            initialization = false;
            StateHasChanged();
        }
    }
    protected async Task RefreshServices(Server server)
    {
        await Task.Yield();

        foreach (var service in server.Services)
        {
            service.IsInChangeState = true;
            StateHasChanged();
            service.Status = ServiceManager.GetServiceStatus(server.Name, service.Name);
            service.IsInChangeState = false;
            StateHasChanged();
        }
    }

    protected async Task RefreshAppPools(Server server)
    {
        await Task.Yield();

        foreach (var appPool in server.AppPools)
        {
            appPool.IsInChangeState = true;
            StateHasChanged();

            if (server.Location == ServerLocationType.Remote)
                appPool.State = PowershellIISManager.GetAppPoolStatus(server.Name, appPool);
            else
                appPool.State = LocalIISManager.GetAppPoolStatus(appPool);

            appPool.IsInChangeState = false;
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

    protected async Task StartAppPoolAndRefreshStateAsync(Server server, AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        try
        {
            StartAppPool(server, appPool);
            await RefreshAppPoolStateAfterStateChangeAsync(server, appPool, ObjectState.Started);

            appPool.State = ObjectState.Started;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            appPool.IsInChangeState = false;
        }
    }

    private void StartAppPool(Server server, AppPool appPool)
    {
        if (server.Location == ServerLocationType.Remote)
            PowershellIISManager.StartAppPool(server.Name, appPool);
        else
            LocalIISManager.StartAppPoolAsync(appPool);
    }

    private async Task RefreshAppPoolStateAfterStateChangeAsync(Server server, AppPool appPool, ObjectState targetState)
    {
        if (server.Location == ServerLocationType.Remote)
            while (PowershellIISManager.GetAppPoolStatus(server.Name, appPool) != targetState)
                await Task.Delay(1000);
        else
            while (LocalIISManager.GetAppPoolStatus(appPool) != targetState)
                await Task.Delay(1000);
    }

    protected async Task StopAppPoolAndRefreshStateAsync(Server server, AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        try
        {
            StopAppPool(server, appPool);
            await RefreshAppPoolStateAfterStateChangeAsync(server, appPool, ObjectState.Stopped);

            appPool.State = ObjectState.Stopped;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            appPool.IsInChangeState = false;
        }
    }

    private void StopAppPool(Server server, AppPool appPool)
    {
        if (server.Location == ServerLocationType.Remote)
            PowershellIISManager.StopAppPool(server.Name, appPool);
        else
            LocalIISManager.StopAppPool(appPool);
    }
}
