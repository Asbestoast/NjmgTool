namespace NjmgLib;
public interface ICharmap : IEnumerable<KeyValuePair<byte, string>>
{
    bool TryGetValue(byte key, out string value);
    string this[byte key]
    {
        get
        {
            if (!TryGetValue(key, out var value))
                throw new KeyNotFoundException();
            return value;
        }
    }
}
