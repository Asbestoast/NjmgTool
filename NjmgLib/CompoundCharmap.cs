using System.Collections;

namespace NjmgLib;
public sealed class CompoundCharmap : ICharmap
{
    public List<ICharmap> Items { get; } = new();

    public bool TryGetValue(byte key, out string value)
    {
        foreach (var charmap in Items)
        {
            if (charmap.TryGetValue(key, out value))
            {
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    public IEnumerator<KeyValuePair<byte, string>> GetEnumerator()
    {
        return Items.SelectMany(charmap => charmap).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static CompoundCharmap FromFiles(IEnumerable<string> files)
    {
        var charmap = new CompoundCharmap();
        foreach (var file in files)
        {
            var table = new Charmap(file);
            charmap.Items.Add(table);
        }
        return charmap;
    }
}
