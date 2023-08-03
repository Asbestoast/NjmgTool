namespace NjmgLib;
internal class ScriptParser : IDisposable
{
    private const string CommentBeginPattern = "##";
    private const string CommandBeginPattern = "#";

    private Stream Stream { get; }
    private StreamReader Reader { get; }
    private int CurrentLine { get; set; }

    public ScriptParser(string path)
    {
        Stream = File.OpenRead(path);
        Reader = new StreamReader(Stream);
    }

    private string? ReadLine()
    {
        var line = Reader.ReadLine();
        if (line != null)
        {
            CurrentLine++;
        }
        return line;
    }

    private Exception MakeSyntaxErrorException(TextParser p, string message)
    {
        return new FormatException($"Syntax error at line {CurrentLine} position {p.Position + 1}: {message}");
    }

    private static bool TryReadParameter(TextParser p, out object result)
    {
        if (p.TryReadNumber(out var numberResult))
        {
            result = numberResult;
            return true;
        }
        else if (p.TryReadString(out var stringResult))
        {
            result = stringResult;
            return true;
        }
        else if (p.TryReadIdentifier(out var identifier))
        {
            result = identifier.ToString();
            return true;
        }
        else
        {
            result = default(int);
            return false;
        }
    }

    public ScriptToken? ReadNext()
    {
        while (true)
        {
            var line = ReadLine();
            if (line == null) return null;
            if (line.Length == 0) continue;
            var p = new TextParser(line);

            if (p.TryReadPattern(CommentBeginPattern))
            {
            }
            else if (p.TryReadPattern(CommandBeginPattern))
            {
                if (!p.TryReadIdentifier(out var commandName))
                    throw MakeSyntaxErrorException(p, "Expected command name.");

                var token = new ScriptToken
                {
                    Type = TokenType.Command,
                    Line = CurrentLine,
                    CommandName = commandName.ToString(),
                };

                while (true)
                {
                    p.ReadCharacters(char.IsWhiteSpace);
                    if (p.IsAtEnd) break;
                    if (p.TryReadPattern(CommentBeginPattern)) break;
                    if (TryReadParameter(p, out var param))
                        token.Parameters.Add(param);
                    else
                        throw MakeSyntaxErrorException(p, "Invalid parameter.");
                }

                return token;
            }
            else
            {
                var token = new ScriptToken
                {
                    Type = TokenType.Content,
                    Content = p.Text,
                    Line = CurrentLine,
                };
                return token;
            }
        }
    }

    public void Dispose()
    {
        Reader.Dispose();
        Stream.Dispose();
    }
}
