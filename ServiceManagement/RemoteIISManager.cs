using Microsoft.Web.Administration;

namespace ServiceManagement;

public interface IRemoteIISManager
{
    void StartAppPoolAsync(string serverName, AppPool appPool);
    void StopAppPoolAsync(string serverName, AppPool appPool);
    ObjectState GetAppPoolStatusAsync(string serverName, AppPool appPool);
}

public class RemoteIISManager : IRemoteIISManager
{
    public void StartAppPoolAsync(string serverName, AppPool appPool)
    {
        using var serverManager = ServerManager.OpenRemote(serverName);
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
        
        if (serverAppPool.State != ObjectState.Started)
            serverAppPool.Start();
    }

    public void StopAppPoolAsync(string serverName, AppPool appPool)
    {
        using var serverManager = ServerManager.OpenRemote(serverName);
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];
     
        if (serverAppPool.State != ObjectState.Stopped)
            serverAppPool.Stop();
    }

    public ObjectState GetAppPoolStatusAsync(string serverName, AppPool appPool)
    {
        using var serverManager = ServerManager.OpenRemote(serverName);
        var serverAppPool = serverManager.ApplicationPools[appPool.Name];

        return serverAppPool.State;
    }
}