using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.Web.Administration;
using ServiceManagement.Services;

namespace ServiceManagement.Components.Pages.ComponentClasses;

public class HomeComponent : ComponentBase
{
    [Inject] protected PreloadService PreloadService { get; set; } = null!;
    [Inject] protected IOptionsSnapshot<ServiceConfig> Config { get; set; } = null!;
}
