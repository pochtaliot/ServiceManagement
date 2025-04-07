using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.Web.Administration;

namespace ServiceManagement.Components.Pages.Partials.AppPools;

public class AppPoolComponentClass : ComponentBase
{
    [Inject] protected IOptionsSnapshot<ServiceConfig> Config { get; set; } = null!;
    [Inject] protected IPowershellIISManager PowershellIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;
    protected bool initialization { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            initialization = true;
            StateHasChanged();

            var refreshTasks = Config.Value.Servers
                .Select(server => RefreshAppPools(server))
                .ToList();

            await Task.WhenAll(refreshTasks);

            initialization = false;
            StateHasChanged();
        }
    }

    protected async Task RefreshAppPools(Server server)
    {
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

        await Task.CompletedTask;
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

    private async Task RefreshAppPoolStateAfterStateChangeAsync(Server server, AppPool appPool, ObjectState targetState)
    {
        if (server.Location == ServerLocationType.Remote)
            while (PowershellIISManager.GetAppPoolStatus(server.Name, appPool) != targetState)
                await Task.Delay(1000);
        else
            while (LocalIISManager.GetAppPoolStatus(appPool) != targetState)
                await Task.Delay(1000);
    }

    private void StartAppPool(Server server, AppPool appPool)
    {
        if (server.Location == ServerLocationType.Remote)
            PowershellIISManager.StartAppPool(server.Name, appPool);
        else
            LocalIISManager.StartAppPoolAsync(appPool);
    }
}
