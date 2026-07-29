using Microsoft.Win32;
using System.Text;

namespace Pakon.Scanner.Legacy;

internal static class PakonNativeRuntimePreflight
{
    private const string TlxInprocServerKey =
        @"CLSID\{EA82986B-E47C-4C0F-97EA-FB50ED216D2E}\InprocServer32";

    private static readonly string[] DependencyNames =
    [
        "msvcr71.dll",
        "msvcp71.dll",
        "ekjpegi.dll",
        "KODAKCMS.dll",
        "xerces-c_2_2_0.dll",
        "ijl15.dll",
        "jpegi.dll"
    ];

    public static void Validate()
    {
        var comServerDirectory = ReadRegisteredTlxDirectory();
        if (string.IsNullOrWhiteSpace(comServerDirectory) || !Directory.Exists(comServerDirectory))
        {
            throw new ScannerPrerequisiteException(
                "The 32-bit Pakon F-X35 COM server is not registered correctly.\n\n" +
                "Repair or reinstall the complete original Pakon F-X35 software and scanner drivers, then start Pakon Client again.");
        }

        var missingFiles = new List<string>();
        if (!File.Exists(Path.Combine(comServerDirectory, "PakonImau.dll")))
            missingFiles.Add("PakonImau.dll");

        var searchDirectories = GetNativeSearchDirectories(comServerDirectory);
        missingFiles.AddRange(DependencyNames.Where(
            name => !searchDirectories.Any(directory => File.Exists(Path.Combine(directory, name)))));

        if (missingFiles.Count == 0) return;

        var message = new StringBuilder()
            .AppendLine("The Pakon F-X35 native runtime is incomplete.")
            .AppendLine()
            .AppendLine("These required 32-bit files could not be found:");

        foreach (var name in missingFiles)
            message.Append("  • ").AppendLine(name);

        message
            .AppendLine()
            .AppendLine("Repair or reinstall the complete original Pakon F-X35 software and scanner drivers, then start Pakon Client again.")
            .AppendLine()
            .AppendLine("The current Microsoft Visual C++ redistributable does not provide the older MSVCR71/MSVCP71 runtime files.")
            .AppendLine()
            .Append("Pakon COM server: ")
            .Append(comServerDirectory);

        throw new ScannerPrerequisiteException(message.ToString());
    }

    private static string? ReadRegisteredTlxDirectory()
    {
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
        using var key = baseKey.OpenSubKey(TlxInprocServerKey);
        var inprocServer = key?.GetValue(null) as string;
        return string.IsNullOrWhiteSpace(inprocServer) ? null : Path.GetDirectoryName(inprocServer);
    }

    private static IReadOnlyList<string> GetNativeSearchDirectories(string comServerDirectory)
    {
        var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var systemDirectory = Environment.Is64BitOperatingSystem
            ? Path.Combine(windowsDirectory, "SysWOW64")
            : Path.Combine(windowsDirectory, "System32");
        var pakonDirectory = Directory.GetParent(comServerDirectory)?.FullName;

        var directories = new[]
            {
                comServerDirectory,
                pakonDirectory,
                systemDirectory,
                windowsDirectory,
                AppContext.BaseDirectory
            }
            .Concat((Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(directory => !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            .Select(directory => directory!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return directories;
    }
}
