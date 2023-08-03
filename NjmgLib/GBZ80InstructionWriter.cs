using System.Text;

namespace NjmgLib;
internal sealed class GBZ80InstructionWriter : IDisposable
{
    private BinaryWriter W { get; }

    public Stream BaseStream => W.BaseStream;

    public GBZ80InstructionWriter(Stream stream, bool leaveOpen = false)
    {
        W = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: leaveOpen);
    }

    public void Dispose() => W.Dispose();

    private void Opcode(byte opcode) => W.Write(opcode);

    private void Opcode(byte opcode, byte literal, out long literalOffset)
    {
        W.Write(opcode);
        literalOffset = BaseStream.Position;
        W.Write(literal);
    }

    private void Opcode(byte opcode, byte literal) => Opcode(opcode, literal, out _);

    private void Opcode(byte opcode, ushort operand, out long literalOffset)
    {
        W.Write(opcode);
        literalOffset = BaseStream.Position;
        W.Write(operand);
    }

    private void Opcode(byte opcode, ushort operand) => Opcode(opcode, operand, out _);

    private void CbOpcode(byte opcode) => Opcode(0xCB, opcode);

    public void ld_b(byte u8) => Opcode(0x06, u8);
    public void ld_hl(ushort u16, out long literalOffset) => Opcode(0x21, u16, out literalOffset);
    public void ld_hl(ushort u16) => ld_hl(u16, out _);
    public void ld_a(byte u8, out long literalOffset) => Opcode(0x3E, u8, out literalOffset);
    public void ld_a(byte u8) => ld_a(u8, out _);
    public void jp(ushort u16) => Opcode(0xC3, u16);
    public void jp(GbPointer u16) => jp(u16.Address);
    public void call(ushort u16) => Opcode(0xCD, u16);
    public void call(GbPointer u16) => call(u16.Address);
    public void sub_a(byte u8) => Opcode(0xD6, u8);
    public void ld_c_a() => Opcode(0x4F);
    public void rl_b() => CbOpcode(0x10);
    public void sla_c() => CbOpcode(0x21);
    public void ldh_a(ushort u8)
    {
        if (u8 >> 8 != 0xFF) throw new ArgumentOutOfRangeException(nameof(u8));
        Opcode(0xF0, (byte)u8);
    }
    public void ldh_a(GbPointer u8) => ldh_a(u8.Address);
}
