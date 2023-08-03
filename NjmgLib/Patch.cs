using System.Text;

namespace NjmgLib;
public sealed class Patch
{
    public static Type DataTypeToType(DataType type)
    {
        return type switch
        {
            DataType.uint8_t => typeof(byte),
            DataType.uint16_t => typeof(ushort),
            DataType.uint32_t => typeof(uint),
            DataType.uint64_t => typeof(ulong),
            DataType.int8_t => typeof(sbyte),
            DataType.int16_t => typeof(short),
            DataType.int32_t => typeof(int),
            DataType.int64_t => typeof(long),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static void WriteValue(Stream stream, object value)
    {
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);

        switch (value)
        {
            case byte byteValue:
                w.Write(byteValue);
                break;
            case ushort ushortValue:
                w.Write(ushortValue);
                break;
            case uint uintValue:
                w.Write(uintValue);
                break;
            case ulong ulongValue:
                w.Write(ulongValue);
                break;
            case sbyte sbyteValue:
                w.Write(sbyteValue);
                break;
            case short shortValue:
                w.Write(shortValue);
                break;
            case int intValue:
                w.Write(intValue);
                break;
            case long longValue:
                w.Write(longValue);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public uint Offset { get; set; }
    public object Value { get; set; } = 0;
}
