using Microsoft.Web.Administration;
using System.Management.Automation.Runspaces;

namespace ServiceManagement;

public interface IPowershellIISManager
{
    void StartAppPool(string serverName, AppPool appPool);
    void StopAppPool(string serverName, AppPool appPool);
    ObjectState GetAppPoolStatus(string serverName, AppPool appPool);
}

public class PowershellIISManager : IPowershellIISManager
{
    public void StartAppPool(string serverName, AppPool appPool)
    {
        ExecutePowerShellCommand(serverName, $"Start-WebAppPool -Name '{appPool.Name}'");
        ExecutePowerShellCommand(serverName, $@"
            Get-Website | Where-Object {{ $_.ApplicationPool -eq '{appPool.Name}' }} | ForEach-Object {{ Start-Website -Name $_.Name }}
        ");
    }

    public void StopAppPool(string serverName, AppPool appPool)
    {
        ExecutePowerShellCommand(serverName, $@"
            Get-Website | Where-Object {{ $_.ApplicationPool -eq '{appPool.Name}' }} | ForEach-Object {{ Stop-Website -Name $_.Name }}
        ");
        ExecutePowerShellCommand(serverName, $"Stop-WebAppPool -Name '{appPool.Name}'");
    }

    public ObjectState GetAppPoolStatus(string serverName, AppPool appPool)
    {
        var result = ExecutePowerShellCommand(serverName, $"(Get-WebAppPoolState -Name '{appPool.Name}').Value");

        return result.Trim() switch
        {
            "Started" => ObjectState.Started,
            "Stopped" => ObjectState.Stopped,
            "Starting" => ObjectState.Starting,
            "Stopping" => ObjectState.Stopping,
            _ => throw new InvalidOperationException("Unknown application pool state: " + result)
        };
    }

    private string ExecutePowerShellCommand(string serverName, string command)
    {
        using var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();

        using var pipeline = runspace.CreatePipeline();
        pipeline.Commands.AddScript($"Invoke-Command -ComputerName {serverName} -ScriptBlock {{{command}}}");

        var results = pipeline.Invoke();
        runspace.Close();

        if (pipeline.Error.Count > 0)
        {
            var errors = string.Join("\n", pipeline.Error.ReadToEnd());
            throw new InvalidOperationException("PowerShell execution failed: " + errors);
        }

        return string.Join("\n", results.Select(r => r.ToString()));
    }
}
