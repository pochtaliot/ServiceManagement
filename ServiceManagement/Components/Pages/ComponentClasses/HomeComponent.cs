using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using ServiceManagement.Services;
using System.Threading.Tasks;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    [Inject] protected IOptionsSnapshot<ServiceConfig> Config { get; set; } = null!;
    [Inject] protected IPowershellIISManager PowershellIISManager { get; set; } = null!;
    [Inject] protected ILocalIISManager LocalIISManager { get; set; } = null!;
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Inject] protected PreloadService PreloadService { get; set; } = null!;
    protected InitializationState InitializationState { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            InitializationState.On = true;
            StateHasChanged();

            var loadServicesTask = Task.Run(() => LoadAllServices());
            var loadAppPoolsTask = Task.Run(() => LoadAllAppPools());

            await Task.WhenAll(loadServicesTask, loadAppPoolsTask);

            await InvokeAsync(() =>
            {
                InitializationState.On = false;
                StateHasChanged();
            });

            StateHasChanged(); 
        }
    }

    private async Task LoadAllServices()
    {
        var loadServerServicesTasks = new List<Task>();

        foreach (var server in Config.Value.Servers.Where(s => s.Services.Any()))
        {
            loadServerServicesTasks.Add(Task.Run(() => LoadServerServices(server)));
        }

        await Task.WhenAll(loadServerServicesTasks);
    }

    private void LoadServerServices(Server server)
    {
        foreach (var service in server.Services)
        {
            var status = ServiceManager.GetServiceStatus(server.Name, service.Name);

            // Use InvokeAsync for UI-bound updates
            InvokeAsync(() =>
            {
                service.IsInChangeState = true;
                StateHasChanged();
                service.Status = status;
                service.IsInChangeState = false;
                StateHasChanged();
            }).Wait(); // Block until UI update completes
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

    private async Task LoadAllAppPools()
    {
        var loadServerAppPoolsTasks = new List<Task>();

        foreach (var server in Config.Value.Servers.Where(s => s.AppPools.Any()))
        {
            loadServerAppPoolsTasks.Add(Task.Run(() => LoadServerAppPools(server)));
        }

        await Task.WhenAll(loadServerAppPoolsTasks);
    }

    private void LoadServerAppPools(Server server)
    {
        foreach (var appPool in server.AppPools)
        {
            var state = server.Location == ServerLocationType.Remote
                ? PowershellIISManager.GetAppPoolStatus(server.Name, appPool)
                : LocalIISManager.GetAppPoolStatus(appPool);

            InvokeAsync(() =>
            {
                appPool.IsInChangeState = true;
                StateHasChanged();
                appPool.State = state;
                appPool.IsInChangeState = false;
                StateHasChanged();
            }).Wait();
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
}
