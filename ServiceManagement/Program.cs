using ServiceManagement;
using ServiceManagement.Components;
using ServiceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ServiceConfig>(builder.Configuration.GetSection("ServiceConfig"));
builder.Services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
builder.Services.AddSingleton<IPowershellIISManager, PowershellIISManager>();
builder.Services.AddSingleton<ILocalIISManager, LocalIISManager>();
builder.Services.AddSingleton<PreloadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

await app.RunAsync();
