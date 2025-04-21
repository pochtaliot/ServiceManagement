using Microsoft.AspNetCore.Components;
using Microsoft.Web.Administration;
using ServiceManagement.Components.Pages.ComponentClasses;

namespace ServiceManagement.Components.Pages.Partials.AppPools;

public class AppPoolComponentClass : ComponentBase
{
    [Inject] protected IPowershellIISManager PowershellIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;
    [Parameter] public IEnumerable<Server> Servers { get; set; } = Enumerable.Empty<Server>();
    [Parameter] public required InitializationState InitializationState { get; set; }
    [Parameter] public EventCallback<(Server Server, AppPool AppPool)> OnRefreshClick { get; set; }

    protected async Task RefreshAppPools(Server server)
    {
        await Task.Yield();

        foreach (var item in server.AppPools)
        {
            item.IsInChangeState = true;
            StateHasChanged();
            await OnRefreshClick.InvokeAsync((server, item));
            item.IsInChangeState = false;
            StateHasChanged();
        }
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
