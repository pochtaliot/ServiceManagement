using System.Collections.Concurrent;
using System.Management;

namespace ServiceManagement;

public class ManagementScopeDispatcher
{
    private readonly ConcurrentDictionary<string, ManagementScope> _scopes = new();

    public ManagementScope GetScope(string serverName)
    {
        return _scopes.GetOrAdd(serverName, server =>
        {
            var scope = new ManagementScope($"\\\\{server}\\root\\cimv2");
            scope.Connect();
            return scope;
        });
    }

    public void RemoveScope(string serverName)
    {
        _scopes.TryRemove(serverName, out var scope);
    }
}
