using Microsoft.Web.Administration;

namespace ServiceManagement;

public interface ILocalIISManager
{
    void StartAppPoolWithSites(AppPool appPool);
    void StopAppPoolWithSites(AppPool appPool);
    ObjectState GetAppPoolStatus(AppPool appPool);
}

public class LocalIISManager : ILocalIISManager
{
    public void StartAppPoolWithSites(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
        
        if (serverAppPool.State != ObjectState.Started)
            serverAppPool.Start();

        foreach (var site in serverManager.Sites.Where(s => s.Applications.Any(a => a.ApplicationPoolName == appPool.Name)))
            if (site.State != ObjectState.Started)
                site.Start();
    }

    public void StopAppPoolWithSites(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];

        if (serverAppPool.State != ObjectState.Stopped)
            serverAppPool.Stop();

        foreach (var site in serverManager.Sites.Where(s => s.Applications.Any(a => a.ApplicationPoolName == appPool.Name)))
            if (site.State != ObjectState.Stopped)
                site.Stop();
    }

    public ObjectState GetAppPoolStatus(AppPool appPool)
    {
        using var serverManager = new ServerManager();
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];

        return serverAppPool.State;
    }
}
