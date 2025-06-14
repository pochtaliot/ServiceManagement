﻿using Microsoft.AspNetCore.Components;
using ServiceManagement.Components.Pages.ComponentClasses;
using System.ServiceProcess;

namespace ServiceManagement.Components.Pages.Partials.Services;

public class ServiceComponentClass : ComponentBase
{
    [Inject] protected IWindowsServiceManager ServiceManager { get; set; } = null!;
    [Inject] protected ManagementScopeDispatcher ScopeDispatcher { get; set; } = null!;
    [Parameter] public IEnumerable<Server> Servers { get; set; } = Enumerable.Empty<Server>();
    [Parameter] public required InitializationState InitializationState { get; set; }
    [Parameter] public EventCallback<(Server Server, Service Service)> OnRefreshClick { get; set; }

    protected async Task RefreshServices(Server server)
    {
        await Task.Yield();
        
        if (server.Services.Any())
        {
            server.Services[0].IsInChangeState = true;
            StateHasChanged(); 
        }

        ScopeDispatcher.AddScope(server.Name);

        foreach (var item in server.Services)
        {
            item.IsInChangeState = true;
            StateHasChanged();
            await OnRefreshClick.InvokeAsync((server, item));
            item.IsInChangeState = false;
            StateHasChanged();
        }

        ScopeDispatcher.RemoveScope(server.Name);
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

    protected async Task ToggleShowArguments(Server server)
    {
        server.ShowStartupArguments = !server.ShowStartupArguments;
        await RefreshServices(server);
    }
}
