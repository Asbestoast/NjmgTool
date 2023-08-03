using System.Diagnostics.Contracts;
using System.Reflection;

namespace NjmgLib;
internal static class PathUtility
{
    [Pure]
    public static string ResolveFilePath(string path)
    {
        if (Path.IsPathFullyQualified(path)) return NormalizePath(path);
        var paths = new[]
        {
            path,
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, path),
        };
        var result = paths.FirstOrDefault(File.Exists);
        if (result == null) throw new IOException($"Failed to resolve path '{path}'");
        return NormalizePath(result);
    }

    [Pure]
    public static string NormalizePath(string path) =>
        path.Replace('\\', '/');
}
