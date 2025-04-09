using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using ServiceManagement.Services;

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

            var loadServicesTask = LoadAllServices();
            var loadAppPoolsTask = LoadAllAppPools();

            await Task.WhenAll(loadServicesTask, loadAppPoolsTask);

            InitializationState.On = false;
            StateHasChanged(); 
        }
    }

    private async Task LoadAllServices()
    {
        var serviceRefreshTasks = Config.Value.Servers
        .Where(server => server.Services.Any())
        .Select(async server =>
        {
            foreach (var service in server.Services)
            {
                service.IsInChangeState = true;
                service.Status = ServiceManager.GetServiceStatus(server.Name, service.Name);
                service.IsInChangeState = false;
            }
            await Task.Yield(); // Ensure async context
        }).ToList();

        await Task.WhenAll(serviceRefreshTasks);
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
        var appPoolRefreshTasks = Config.Value.Servers
        .Where(server => server.AppPools.Any())
        .Select(async server =>
        {
            foreach (var appPool in server.AppPools)
            {
                appPool.IsInChangeState = true;
                appPool.State = server.Location == ServerLocationType.Remote
                    ? PowershellIISManager.GetAppPoolStatus(server.Name, appPool)
                    : LocalIISManager.GetAppPoolStatus(appPool);
                appPool.IsInChangeState = false;
            }
            await Task.Yield(); // Ensure async context
        }).ToList();

        await Task.WhenAll(appPoolRefreshTasks);
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
