namespace NjmgLib;
internal sealed class TextParser
{
    public string Text { get; set; }
    public int Position { get; set; }
    public bool IsAtEnd => Position >= Text.Length;

    public bool TryReadCharacter(Func<char, bool> filter, out char result)
    {
        var oldPosition = Position;
        if (TryReadCharacter(out result) && filter(result))
            return true;
        Position = oldPosition;
        result = default;
        return false;
    }

    public ReadOnlySpan<char> ReadCharacters(Func<char, bool> filter)
    {
        var start = Position;
        while (TryReadCharacter(filter, out _))
        {
        }
        return Text.AsSpan(start, Position - start);
    }

    public bool TryReadCharacter(out char result)
    {
        if (Position < 0 || IsAtEnd)
        {
            result = default;
            return false;
        }

        result = Text[Position++];
        return true;
    }

    public bool TryReadPattern(ReadOnlySpan<char> pattern)
    {
        var oldPosition = Position;

        foreach (var c in pattern)
        {
            if (TryReadCharacter(out var c2) && c2 == c)
                continue;
            Position = oldPosition;
            return false;
        }

        return true;
    }

    public TextParser() : this(string.Empty)
    {
    }

    public TextParser(string text)
    {
        Text = text;
    }
}