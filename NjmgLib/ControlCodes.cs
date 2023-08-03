namespace NjmgLib;

internal static class ControlCodes
{
    public const int Word0 = 0x00;
    /// <summary>
    /// Shows an 8-bit number.
    /// </summary>
    public const int NumberVariable = 0x01;
    /// <summary>
    /// Shows a 16-bit number.
    /// </summary>
    public const int NumberVariable16 = 0x02;
    public const int Word1Variable = 0x03;
    public const int StringVariable = 0x04;
    /// <summary>
    /// Shows a 16-bit number with less padding.
    /// </summary>
    public const int SmallNumberVariable16 = 0x05;
    /// <summary>
    /// Moves the insertion point to the center of the current line.
    /// </summary>
    public const int Center = 0x06;
    public const int Slow = 0x08;
    public const int Fast = 0x09;
    public const int WaitKeyAndClear = 0x0A;
    public const int WaitKey = 0x0B;
    public const int Clear = 0x0C;
    public const int Newline = 0x0D;
    public const int DrawTilemap = 0x0E;
    public const int Word0Variable = 0x12;
    public const int Word1 = 0x13;
    public const int Position = 0x22;
    public const int End = 0xFF;
}