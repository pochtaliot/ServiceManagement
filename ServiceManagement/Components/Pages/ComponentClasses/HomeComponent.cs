using Microsoft.AspNetCore.Components;
using ServiceManagement.Services;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    [Inject] protected PreloadService PreloadService { get; set; } = null!;

    protected bool componentsLoaded = false;

    protected override async Task OnInitializedAsync()
    {
        // Start loading both components in parallel
        var serviceTask = Task.Run(() => componentsLoaded = true);
        var appPoolTask = Task.Run(() => componentsLoaded = true);

        await Task.WhenAll(serviceTask, appPoolTask);
        StateHasChanged();
    }
}
