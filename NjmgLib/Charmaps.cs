namespace NjmgLib;
public static class Charmaps
{
    public static string JpSource { get; } = PathUtility.ResolveFilePath(Path.Combine(Constants.ScriptsDirectory, "jp/charmap.map"));
    public static ICharmap Jp => _jp ??= new Charmap(JpSource);
    private static ICharmap? _jp;

    public static string JpRawSource { get; } = PathUtility.ResolveFilePath(Path.Combine(Constants.ScriptsDirectory, "jp/raw.map"));
    public static ICharmap JpRaw => _jpRaw ??= new Charmap(JpRawSource);
    private static ICharmap? _jpRaw;
}
