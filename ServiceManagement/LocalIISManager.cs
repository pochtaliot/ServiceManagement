using Microsoft.Web.Administration;

namespace ServiceManagement;

public interface ILocalIISManager
{
    void StartAppPoolAsync(AppPool appPool);
    void StopAppPool(AppPool appPool);
    ObjectState GetAppPoolStatus(AppPool appPool);
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

    public void StopAppPool(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
        if (serverAppPool.State != ObjectState.Stopped)
        {
            serverAppPool.Stop();
        }
    }

    public ObjectState GetAppPoolStatus(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];

        return serverAppPool.State;
    }
}
