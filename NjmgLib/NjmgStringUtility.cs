using System.Diagnostics.Contracts;
using System.Text;

namespace NjmgLib;
internal static class NjmgStringUtility
{
    private const char DictionaryBeginPattern = '{';
    private const char DictionaryEndPattern = '}';
    private const string RawValueBeginPattern = "<#";
    private const string RawValueContinuationPattern = "#";
    private const string RawValueEndPattern = ">";

    [Pure]
    public static GbPointer GetDictionaryStringPointer(int index)
    {
        return GbPointerUtility.GetArrayItemPointer(Constants.WordTable.Address, index, Constants.MaxWordLength);
    }

    private static bool TryReadRawStringValue(TextParser p, Stream stream)
    {
        var oldPosition = p.Position;
        if (!p.TryReadPattern(RawValueBeginPattern)) return false;

        var size = 1;
        while (p.TryReadPattern(RawValueContinuationPattern))
        {
            size++;
        }

        if (p.TryReadNumber(out var value))
        {
            using var w = new BinaryWriter(stream, Encoding.ASCII, true);
            if (size == 1)
                w.Write(checked((byte)value));
            else if (size == 2)
                w.Write(checked((ushort)value));
            else if (size == 3)
                w.Write(checked((uint)value));
            else
                throw new FormatException($"Unsupported raw value size {size}.");
            if (!p.TryReadPattern(RawValueEndPattern))
                throw new FormatException("Expected end of raw byte pattern.");
            return true;
        }
        else
        {
            p.Position = oldPosition;
        }

        return false;
    }

    /// <summary>
    /// Encodes a string in NJMG format.
    /// A terminating byte is not included in the resulting data.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="charmap"></param>
    /// <param name="includeTerminator"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Pure]
    public static byte[] EncodeString(string str, ICharmap charmap, bool includeTerminator = false)
    {
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);

        str = NormalizeDictionaryLookups(str);

        var p = new TextParser(str);

        while (!p.IsAtEnd)
        {
            if (TryReadRawStringValue(p, stream))
            {
            }
            else
            {
                var foundMapping = false;
                foreach (var mapping in charmap)
                {
                    if (!p.TryReadPattern(mapping.Value)) continue;
                    w.Write(mapping.Key);
                    foundMapping = true;
                    break;
                }
                if (!foundMapping)
                {
                    throw new FormatException($"No mapping found for '{p.Text[p.Position]}'.");
                }
            }
        }

        if (includeTerminator)
        {
            w.Write((byte)ControlCodes.End);
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Converts {$nn} word-lookup syntax into &lt;$nn&gt; byte literal tags.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Pure]
    private static string NormalizeDictionaryLookups(string value)
    {
        var sb = new StringBuilder();

        var p = new TextParser(value);

        while (!p.IsAtEnd)
        {
            sb.Append(p.ReadCharacters(c => c != DictionaryBeginPattern));
            if (p.TryReadPattern(DictionaryBeginPattern.ToString()))
            {
                if (!p.TryReadNumber(out var index))
                    throw new FormatException("Expected number.");
                p.ReadCharacters(c => c != DictionaryEndPattern);
                if (!p.TryReadPattern(DictionaryEndPattern.ToString()))
                    throw new FormatException("Expected end of dictionary reference.");
                sb.Append(MakeRawValueControlCode(index < 0x100 ? (byte)ControlCodes.Word0 : (byte)ControlCodes.Word1));
                sb.Append(MakeRawValueControlCode((byte)(index & 0xFF)));
            }
        }

        return sb.ToString();
    }

    [Pure]
    public static string MakeRawValueControlCode(byte value)
    {
        return $"<#${value:X}>";
    }

    [Pure]
    public static string MakeRawValueControlCode(ushort value)
    {
        return $"<##${value:X}>";
    }

    public static string DecodeString(
        Stream stream, ICharmap charmap, TextDecodeOptions options)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        var sb = new StringBuilder();

        for (var i = 0; options.MaxLength < 0 || i < options.MaxLength; i++)
        {
            var b = r.ReadByte();
            if (b == ControlCodes.Word0 || b == ControlCodes.Word1)
            {
                var wordIndex = (int)r.ReadByte();
                if (b == ControlCodes.Word1) wordIndex += 0x100;
                if (options.ExpandDictionaryWords)
                {
                    var oldPosition = stream.Position;
                    var word = GetDictionaryString(wordIndex, charmap, stream);
                    stream.Position = oldPosition;
                    sb.Append($"{{${wordIndex:X} {word}}}");
                }
                else
                {
                    sb.Append($"{{${wordIndex:X}}}");
                }
            }
            else if (b == ControlCodes.End)
            {
                break;
            }
            else
            {
                if (charmap.TryGetValue(b, out var c))
                {
                    sb.Append(c);
                }
                else
                {
                    Console.WriteLine($"Warning: No charmap entry for ${b:X}.");
                    sb.Append(MakeRawValueControlCode(b));
                }

                if (b == ControlCodes.NumberVariable ||
                    b == ControlCodes.NumberVariable16 ||
                    b == ControlCodes.Word1Variable ||
                    b == ControlCodes.StringVariable ||
                    b == ControlCodes.SmallNumberVariable16 |
                    b == ControlCodes.Word0Variable ||
                    b == ControlCodes.DrawTilemap)
                {
                    sb.Append(MakeRawValueControlCode(r.ReadUInt16()));
                }
                else if (b == ControlCodes.Position ||
                         (b == ControlCodes.Clear && options.IsWindow))
                {
                    sb.Append(MakeRawValueControlCode(r.ReadByte()));
                    sb.Append(MakeRawValueControlCode(r.ReadByte()));
                }

                if (b == ControlCodes.Newline)
                {
                    sb.AppendLine();
                }
                else if (b == ControlCodes.WaitKeyAndClear || b == ControlCodes.Clear)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }

    public static string DecodeRawString(Stream stream, ICharmap charmap, int length, bool warningsEnabled = true)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        var sb = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var b = r.ReadByte();

            if (charmap.TryGetValue(b, out var c))
            {
                sb.Append(c);
            }
            else
            {
                if (warningsEnabled) Console.WriteLine($"Warning: No charmap entry for ${b:X}.");
                sb.Append(MakeRawValueControlCode(b));
            }
        }

        return sb.ToString();
    }

    public static List<string> DecodeTilemapString(Stream stream, ICharmap charmap)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        var width = r.ReadByte();
        var height = r.ReadByte();

        var results = new List<string>();

        for (var y = 0; y < height; y++)
        {
            var line = DecodeRawString(stream, charmap, width);
            results.Add(line);
        }

        return results;
    }

    public static byte[] EncodeRawString(string str, ICharmap charmap)
    {
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);

        var p = new TextParser(str);

        while (!p.IsAtEnd)
        {
            if (TryReadRawStringValue(p, stream))
            {
            }
            else
            {
                var foundMapping = false;
                foreach (var mapping in charmap)
                {
                    if (!p.TryReadPattern(mapping.Value)) continue;
                    w.Write(mapping.Key);
                    foundMapping = true;
                    break;
                }
                if (!foundMapping)
                {
                    throw new FormatException($"No mapping found for '{p.Text[p.Position]}'.");
                }
            }
        }

        return stream.ToArray();
    }

    [Pure]
    public static string GetDictionaryString(
        int index, ICharmap charmap, Stream stream, bool expandDictionaryWords = false)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        r.BaseStream.Position = GetDictionaryStringPointer(index).ToAbsoluteRomAddress();
        var options = new TextDecodeOptions
        {
            ExpandDictionaryWords = expandDictionaryWords,
            MaxLength = Constants.MaxWordLength,
        };
        return DecodeString(stream, charmap, options);
    }

    [Pure]
    public static TextDecodeOptions GetTextDecodeOptions(TextTableInfo tableInfo)
    {
        return new TextDecodeOptions
        {
            IsWindow = tableInfo.ContainsWindowStrings,
        };
    }
}
