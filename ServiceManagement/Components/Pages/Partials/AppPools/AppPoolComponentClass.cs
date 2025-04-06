using Microsoft.AspNetCore.Components;
using Microsoft.Web.Administration;

namespace ServiceManagement.Components.Pages.Partials.AppPools;

public class AppPoolComponentClass : ComponentBase
{
    [Inject] protected IPowershellIISManager PowershellIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;
    [Parameter] public Server Server { get; set; } = null!;
    protected bool initialization { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            initialization = true;
            StateHasChanged();
            await RefreshAppPools();
            initialization = false;
            StateHasChanged();
        }
    }

    protected async Task RefreshAppPools()
    {
        foreach (var appPool in Server.AppPools)
        {
            appPool.IsInChangeState = true;
            StateHasChanged();

            if (Server.Location == ServerLocationType.Remote)
                appPool.State = PowershellIISManager.GetAppPoolStatus(Server.Name, appPool);
            else
                appPool.State = LocalIISManager.GetAppPoolStatus(appPool);

            appPool.IsInChangeState = false;
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    protected async Task StartAppPoolAndRefreshStateAsync(AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        try
        {
            StartAppPool(Server, appPool);
            await RefreshAppPoolStateAfterStateChangeAsync(appPool, ObjectState.Started);

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

    protected async Task StopAppPoolAndRefreshStateAsync(AppPool appPool)
    {
        appPool.IsInChangeState = true;
        await Task.Yield();

        try
        {
            StopAppPool(Server, appPool);
            await RefreshAppPoolStateAfterStateChangeAsync(appPool, ObjectState.Stopped);

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

    private async Task RefreshAppPoolStateAfterStateChangeAsync(AppPool appPool, ObjectState targetState)
    {
        if (Server.Location == ServerLocationType.Remote)
            while (PowershellIISManager.GetAppPoolStatus(Server.Name, appPool) != targetState)
                await Task.Delay(1000);
        else
            while (LocalIISManager.GetAppPoolStatus(appPool) != targetState)
                await Task.Delay(1000);
    }

    private void StartAppPool(Server server, AppPool appPool)
    {
        if (Server.Location == ServerLocationType.Remote)
            PowershellIISManager.StartAppPool(Server.Name, appPool);
        else
            LocalIISManager.StartAppPoolAsync(appPool);
    }
}
