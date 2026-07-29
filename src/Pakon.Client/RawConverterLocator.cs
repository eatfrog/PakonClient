using System.IO;

namespace Pakon.Client;

internal static class RawConverterLocator
{
    public static string Find()
    {
        var candidates = CandidateRoots().SelectMany(root => new[]
        {
            Path.Combine(root, "src", "Pakon.RawImageConverter", "bin", "Debug", "net10.0", "Pakon.RawImageConverter.dll"),
            Path.Combine(root, "src", "Pakon.RawImageConverter", "bin", "Release", "net10.0", "Pakon.RawImageConverter.dll"),
            Path.Combine(AppContext.BaseDirectory, "Pakon.RawImageConverter.dll")
        });
        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Pakon.RawImageConverter.dll was not found. Build Pakon.sln first.");
    }

    private static IEnumerable<string> CandidateRoots()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Pakon.sln")))
                yield return current.FullName;
            current = current.Parent;
        }
        yield return @"C:\Code\PakonClient";
    }
}
