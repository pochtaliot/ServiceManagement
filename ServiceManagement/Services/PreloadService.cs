namespace ServiceManagement.Services;

public class PreloadService
{
    public bool WaitingServiceStateChange { get; private set; }
    public bool WaitingAppPoolStateChange { get; private set; }

    public async Task ShowWaitingService()
    {
        WaitingServiceStateChange = true;
        await Task.Yield();
    }
    public async Task HideWaitingService()
    {
        WaitingServiceStateChange = false;
        await Task.Yield();
    }

    public async Task ShowWaitingAppPool()
    {
        WaitingAppPoolStateChange = true;
        await Task.Yield();
    }

    public async Task HideWaitingAppPool()
    {
        WaitingAppPoolStateChange = false;
        await Task.Yield();
    }
}
