using Microsoft.AspNetCore.Components;
using ServiceManagement.Services;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    [Inject] protected PreloadService PreloadService { get; set; } = null!;
}
