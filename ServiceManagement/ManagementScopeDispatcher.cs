using System.Collections.Concurrent;
using System.Management;

namespace ServiceManagement;

public class ManagementScopeDispatcher
{
    private readonly ConcurrentDictionary<string, ManagementScope> _scopes = new();

    public void AddScope(string serverName)
    {
        if (!_scopes.ContainsKey(serverName))
        {
            var scope = new ManagementScope($"\\\\{serverName}\\root\\cimv2");
            scope.Connect();
            _scopes.TryAdd(serverName, scope);
        }
    }

    /// <summary>
    /// Gets ManagementScope, previously created and added to _scopes dictionary.
    /// </summary>
    /// <param name="serverName"></param>
    /// <returns></returns>
    public ManagementScope GetScope(string serverName)
    {
        _scopes.TryGetValue(serverName, out var scope);
        return scope;
    }
    public void RemoveScope(string serverName) => _scopes.TryRemove(serverName, out var scope);
}
