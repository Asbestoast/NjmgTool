using System.Text.RegularExpressions;

namespace NjmgLib;
internal static class Utility
{
    private static Regex LineSplitRegex =  new("\r\n|\r|\n", RegexOptions.Compiled);

    public static string[] SplitLines(this string self)
    {
        return LineSplitRegex.Split(self);
    }

    public static uint Sum(this IEnumerable<uint> self)
    {
        return self.Aggregate<uint, uint>(0, (current, item) => current + item);
    }

    public static TValue GetValueOrNew<TKey, TValue>(
        this IDictionary<TKey, TValue> self, TKey key) where TValue : new()
    {
        if (!self.TryGetValue(key, out var value))
        {
            value = new TValue();
            self.Add(key, value);
        }

        return value;
    }

    /// <summary>
    /// Finds the index of the sequence <paramref name="other"/> in sequence <paramref name="self"/>.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <returns>The index of sequence <paramref name="other"/> in sequence <paramref name="self"/>, or -1 if the sequence was not found.</returns>
    public static int SequenceIndexOf(this byte[] self, byte[] other)
    {
        if (other.Length == 0) return 0;
        for (var i = 0; i <= self.Length - other.Length; i++)
        {
            var foundMatch = true;
            for (var j = 0; j < other.Length; j++)
            {
                if (other[j] != self[i + j])
                {
                    foundMatch = false;
                    break;
                }
            }
            if (foundMatch)
            {
                return i;
            }
        }
        return -1;
    }
}
