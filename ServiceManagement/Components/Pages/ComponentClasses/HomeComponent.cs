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

    protected void LoadServerServices(Server server)
    {
        foreach (var service in server.Services)
        {
            try
            {
                var status = ServiceManager.GetServiceStatus(server.Name, service.Name);

                // Use InvokeAsync for UI-bound updates
                InvokeAsync(() => AssignStatusOrFailResult(service, () => service.Status = status))
                    .Wait(); // Block until UI update completes
            }
            catch
            {
                InvokeAsync(() => AssignStatusOrFailResult(service, () => service.StateRetrievedSuccessfully = false))
                    .Wait(); 
            }
        }
    }

    private void AssignStatusOrFailResult(Service service, Action action)
    {
        service.IsInChangeState = true;
        StateHasChanged();
        action.Invoke();
        service.IsInChangeState = false;
        StateHasChanged();
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

    protected void LoadServerAppPools(Server server)
    {
        foreach (var appPool in server.AppPools)
        {
            try
            {
                var state = server.Location == ServerLocationType.Remote
                        ? PowershellIISManager.GetAppPoolStatus(server.Name, appPool)
                        : LocalIISManager.GetAppPoolStatus(appPool);

                InvokeAsync(() => AssignStatusOrFailResult(appPool, () => appPool.State = state))
                    .Wait();
            }
            catch
            {
                InvokeAsync(() => AssignStatusOrFailResult(appPool, () => appPool.StateRetrievedSuccessfully = false))
                    .Wait();
            }
        }
    }

    private void AssignStatusOrFailResult(AppPool appPool, Action action)
    {
        appPool.IsInChangeState = true;
        StateHasChanged();
        action.Invoke();
        appPool.IsInChangeState = false;
        StateHasChanged();
    }
}
