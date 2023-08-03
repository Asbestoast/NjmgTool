using System.Collections;
using System.Globalization;

namespace NjmgLib;
public sealed class Charmap : ICharmap
{
    public Dictionary<byte, string> Mappings { get; } = new();

    public Charmap()
    {
    }

    public Charmap(string path)
    {
        using var file = File.OpenText(path);
        while (true)
        {
            var line = file.ReadLine();
            if (line == null) break;
            if (line.Length == 0) continue;
            var lineParts = line.Split('=');
            if (lineParts.Length != 2)
                throw new IOException("Invalid format.");
            var key = byte.Parse(lineParts[0], NumberStyles.HexNumber);
            var value = lineParts[1];
            Mappings.Add(key, value);
        }
    }

    public bool TryGetValue(byte key, out string value)
    {
        var result = Mappings.TryGetValue(key, out var tempValue);
        value = tempValue ?? string.Empty;
        return result;
    }

    public IEnumerator<KeyValuePair<byte, string>> GetEnumerator()
    {
        return Mappings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
