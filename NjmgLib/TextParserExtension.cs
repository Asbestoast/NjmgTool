using System.Globalization;
using System.Text;

namespace NjmgLib;
internal static class TextParserExtension
{
    private const string HexadecimalPrefixPattern = "$";
    private const string StringBeginPattern = "\"";
    private const string StringEndPattern = "\"";
    private const string StringEscapeBeginPattern = "\\";

    public static bool TryReadNumber(this TextParser p, out long result)
    {
        var oldPosition = p.Position;

        ReadOnlySpan<char> digits;
        NumberStyles numberStyle;

        if (p.TryReadPattern(HexadecimalPrefixPattern))
        {
            digits = p.ReadCharacters(char.IsAsciiHexDigit);
            numberStyle = NumberStyles.HexNumber;
        }
        else
        {
            digits = p.ReadCharacters(char.IsAsciiDigit);
            numberStyle = NumberStyles.Number;
        }

        if (digits.Length == 0) goto fail;
        if (!long.TryParse(digits, numberStyle, CultureInfo.InvariantCulture, out result)) goto fail;
        return true;

    fail:
        p.Position = oldPosition;
        result = default;
        return false;
    }

    public static bool TryReadIdentifier(this TextParser p, out ReadOnlySpan<char> result)
    {
        var oldPosition = p.Position;

        if (p.ReadCharacters(i => char.IsAsciiLetter(i) || i == '_').IsEmpty) goto fail;
        p.ReadCharacters(i => char.IsAsciiLetter(i) || char.IsAsciiDigit(i) || i == '_');
        result = p.Text.AsSpan(oldPosition, p.Position - oldPosition);
        return true;

    fail:
        p.Position = oldPosition;
        result = default;
        return false;
    }

    public static bool TryReadString(this TextParser p, out string result)
    {
        var oldPosition = p.Position;

        if (!p.TryReadPattern(StringBeginPattern)) goto fail;

        var sb = new StringBuilder();

        while (true)
        {
            if (p.TryReadPattern(StringEscapeBeginPattern))
            {
                var pattern = StringEscapeEndPatterns
                    .FirstOrDefault(i => p.TryReadPattern(i.Key));
                if (pattern.Key == null) goto fail;
                sb.Append(pattern.Value);
            }
            else if (p.TryReadPattern(StringEndPattern))
            {
                break;
            }
            else
            {
                if (!p.TryReadCharacter(out var c)) break;
                sb.Append(c);
            }
        }

        result = sb.ToString();
        return true;

    fail:
        p.Position = oldPosition;
        result = string.Empty;
        return false;
    }

    private static List<KeyValuePair<string, string>> StringEscapeEndPatterns { get; } = new()
    {
        new("\"", "\""),
        new("\\", "\\"),
    };

    public static string MakeQuotedString(string str)
    {
        foreach (var pattern in StringEscapeEndPatterns)
        {
            str = str.Replace(pattern.Value, $"{StringEscapeBeginPattern}{pattern.Key}");
        }
        return $"\"{str}\"";
    }
}
