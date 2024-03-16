using System.Diagnostics;
using System.Reflection;

namespace SampleApi;

public static class Telemetry
{
    private static readonly AssemblyName _executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
    public static readonly string ApplicationName = _executingAssemblyName.Name ?? "Noname";
    public static readonly string ApplicationVersion = _executingAssemblyName.Version?.ToString() ?? "0.0.0.0";
    public static readonly ActivitySource ApplicationActivitySource = new(ApplicationName);
}
