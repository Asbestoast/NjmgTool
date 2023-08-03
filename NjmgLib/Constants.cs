using System.Drawing;

namespace NjmgLib;

internal static class Constants
{
    public const string ScriptsDirectory = "scripts";

    public const string NekojaraMonogatariJapan_SHA256 = "8163258B8B0A5D7F6E421D3582EC209950B40E93A195DAE9534952ED473B485A";

    public static TextTableInfo[] DialogueTables { get; } =
    {
        new(0x0, new GbPointer(3, 0x4000), 0x100)
        {
            References =
            {
                new(new GbPointer(0, 0x1557 + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x1565 + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
        new(0x100, new GbPointer(3, 0x4200), 0x100)
        {
            References =
            {
                new(new GbPointer(0, 0x1536 + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x1544 + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
        new(0x200, new GbPointer(2, 0x4000), 0x80)
        {
            References =
            {
                new(new GbPointer(0, 0x1589 + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x1597 + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
            IsSplittable = true,
            SplitFunctionReferences =
            {
                new(new GbPointer(0, 0x1583).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
        new(0x300, new GbPointer(2, 0x4100), 0x40)
        {
            References =
            {
                new(new GbPointer(0, 0x159C + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x15AC + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
        new(0x400, new GbPointer(5, 0x6AD2), 0x40)
        {
            References =
            {
                new(new GbPointer(0, 0x15B1 + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x15C1 + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
        new(0x500, new GbPointer(4, 0x5F30), 0x26)
        {
            IgnoresPlaceholderText = true,
            IsRelocatable = false,
            ContainsWindowStrings = true,
            References =
            {
                new(new GbPointer(0, 0x1506 + 1).ToAbsoluteRomAddress(), ReferenceType.Bank),
                new(new GbPointer(0, 0x1512 + 1).ToAbsoluteRomAddress(), ReferenceType.Address),
            },
        },
    };

    public static TableInfo WordTable { get; } = new(new GbPointer(5, 0x4000), 0x110);
    public const int MaxWordLength = 8;

    public const int PlayerNameWordIndex = 0x50;
    public static GbPointer DefaultPlayerName { get; } = new(1, 0x7155);

    public static Dictionary<int, TilemapInfo> TilemapInfos { get; } = new()
    {
        // Startup splash screen (line 0)
        { 0, new TilemapInfo(new GbPointer(1, 0x6E73), new GbPointer(1, 0x6E76)) },
        // Startup splash screen (line 1)
        { 1, new TilemapInfo(new GbPointer(1, 0x6E7C), new GbPointer(1, 0x6E7F)) },
        // Startup splash screen (line 2)
        { 2, new TilemapInfo(new GbPointer(1, 0x6E85), new GbPointer(1, 0x6E88)) },

        // Game ending text ("ハッピー　エンド")
        { 3, new TilemapInfo(new GbPointer(1, 0x782E), new GbPointer(1, 0x7831)) },
    };

    public static Dictionary<int, BackgroundTilemapInfo> BackgroundTilemapInfos { get; } = new()
    {
        { 0, new BackgroundTilemapInfo(new GbPointer(1, 0x6EE0), Array.Empty<string>()) }, // Title screen
        { 1, new BackgroundTilemapInfo(new GbPointer(1, 0x718B), new[] { Charmaps.JpRawSource }) }, // Name entry screen
    };

    public static Dictionary<int, TableInfo> MenuTables { get; } = new()
    {
        { 0, new TableInfo(new GbPointer(1, 0x7287), 0x4E) }, // Name entry
    };

    public static Size TileSize { get; } = new(8, 8);

    public static Range[] UnusedSpace { get; } =
    {
        new(0x3, 0x7 + 1), // RST 0 padding
        new(0xB, 0xF + 1), // RST 8 padding
        new(0x13, 0x3F + 1), // RST 10 padding, RST 18, RST 20, RST 28, RST 30, RST 38
        new(0x43, 0x47 + 1), // VBlank interrupt handler padding
        new(0x48, 0x48 + 1), // LCD interrupt handler
        new(0x49, 0x4F + 1), // LCD interrupt handler padding
        new(0x50, 0x50 + 1), // Timer interrupt handler
        new(0x51, 0x57 + 1), // Timer interrupt handler padding
        new(0x58, 0x58 + 1), // Serial interrupt handler
        new(0x59, 0x5F + 1), // Serial interrupt handler padding
        new(0x60, 0x60 + 1), // Joypad interrupt handler
        new(0x61, 0xFF + 1), // Joypad interrupt handler padding
        new(0x3FCC, 0x3FFF + 1), // End padding (ROM0)
        new(0x7F91, 0x7FFF + 1), // End padding (ROM1)
        new(0xBFF9, 0xBFFF + 1), // End padding (ROM2)
        new(0xFFD6, 0xFFFF + 1), // End padding (ROM3)
        new(0x13FE0, 0x13FFF + 1), // End padding (ROM4)
        new(0x17FCC, 0x17FFF + 1), // End padding (ROM5)
    };

    public static GbPointer system_pushRomBank { get; } = new(0, 0x10A8);
    public static GbPointer dialogue_showDialogueAndPopRomBank { get; } = new(0, 0x15C4);
    public static GbPointer dialogue_nextDialogueIndex { get; } = new(0, 0xFFD4);
}