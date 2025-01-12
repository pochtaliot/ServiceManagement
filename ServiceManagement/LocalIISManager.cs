using Microsoft.Web.Administration;

namespace ServiceManagement;

public interface ILocalIISManager
{
    void StartAppPoolAsync(AppPool appPool);
    void StopAppPoolAsync(AppPool appPool);
    ObjectState GetAppPoolStatusAsync(AppPool appPool);
}

public class LocalIISManager : ILocalIISManager
{
    public void StartAppPoolAsync(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
        if (serverAppPool.State != ObjectState.Started)
        {
            serverAppPool.Start();
        }
    }

    public void StopAppPoolAsync(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
        if (serverAppPool.State != ObjectState.Stopped)
        {
            serverAppPool.Stop();
        }
    }

    public ObjectState GetAppPoolStatusAsync(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];

        return serverAppPool.State;
    }
}
