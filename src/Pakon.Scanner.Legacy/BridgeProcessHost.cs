using System.Diagnostics;

namespace Pakon.Scanner.Legacy;

internal sealed class BridgeProcessHost : IDisposable
{
    private Process? process;

    public void EnsureStarted()
    {
        if (process is { HasExited: false }) return;
        process?.Dispose();
        process = null;
        var bridge = FindBridge();
        var dotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "dotnet", "dotnet.exe");
        if (!File.Exists(dotnet)) throw new FileNotFoundException("The 32-bit .NET runtime is required.", dotnet);
        process = Process.Start(new ProcessStartInfo
        {
            FileName = dotnet,
            Arguments = $"\"{bridge}\" --pipe PakonLegacyBridge",
            WorkingDirectory = Path.GetDirectoryName(bridge)!,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Normal
        }) ?? throw new InvalidOperationException("Could not start the Pakon legacy bridge.");
    }

    public void RestartOwnedOrStart()
    {
        if (process is { HasExited: false })
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(3000);
        }
        process?.Dispose();
        process = null;
        EnsureStarted();
    }

    private static string FindBridge()
    {
        var candidates = CandidateRoots().SelectMany(root => new[]
        {
            Path.Combine(root, "src", "Pakon.LegacyBridge", "bin", "x86", "Debug", "net10.0-windows", "Pakon.LegacyBridge.dll"),
            Path.Combine(root, "src", "Pakon.LegacyBridge", "bin", "x86", "Release", "net10.0-windows", "Pakon.LegacyBridge.dll"),
            Path.Combine(root, "src", "Pakon.LegacyBridge", "bin", "Debug", "net10.0-windows", "Pakon.LegacyBridge.dll"),
            Path.Combine(root, "src", "Pakon.LegacyBridge", "bin", "Release", "net10.0-windows", "Pakon.LegacyBridge.dll"),
            Path.Combine(AppContext.BaseDirectory, "Pakon.LegacyBridge.dll")
        });
        return candidates
            .Where(File.Exists)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("Pakon.LegacyBridge.dll was not found. Build Pakon.sln first.");
    }

    private static IEnumerable<string> CandidateRoots()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Pakon.sln"))) yield return current.FullName;
            current = current.Parent;
        }
        yield return @"C:\Code\PakonClient";
    }

    public void Dispose()
    {
        if (process is { HasExited: false })
        {
            process.Kill(entireProcessTree: true);
            process.Dispose();
        }
    }
}
