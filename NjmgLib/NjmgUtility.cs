using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using NjmgLib.Graphics;

namespace NjmgLib;
public static class NjmgUtility
{
    public static void ExtractScriptFromRom(
        string srcPath, string dstPath,
        bool debugEnabled = false,
        bool placeholderTextEnabled = false)
    {
        var charmap = Charmaps.Jp;

        var bytes = File.ReadAllBytes(PathUtility.ResolveFilePath(srcPath));
        using var stream = new MemoryStream(bytes);
        using var r = new BinaryReader(stream, Encoding.ASCII, true);

        using var dst = File.Open(dstPath, FileMode.Create, FileAccess.Write);
        using var w = new StreamWriter(dst, Encoding.UTF8, leaveOpen: true);

        var sha256 = Constants.NekojaraMonogatariJapan_SHA256;
        CheckHash(bytes, new[] { sha256 });

        w.WriteLine($"#{CommandNames.Script}");
        var version = new Version(1, 0, 0, 0);
        w.WriteLine($"#{CommandNames.Version} {version.Major} {version.Minor} {version.Build} {version.Revision}");
        w.WriteLine($"#{CommandNames.Target} {TextParserExtension.MakeQuotedString(sha256)} {TextParserExtension.MakeQuotedString(Path.GetFileName(srcPath))}");
        w.WriteLine($"#{CommandNames.Charmap} {TextParserExtension.MakeQuotedString(Charmaps.JpSource)}");

        w.WriteLine();

        var linePrefix = string.Empty;
        if (placeholderTextEnabled) linePrefix = "##";

        foreach (var tableInfo in Constants.DialogueTables)
        {
            var stringDecodeOptions = NjmgStringUtility.GetTextDecodeOptions(tableInfo);
            stringDecodeOptions.ExpandDictionaryWords = true;

            for (var i = 0; i < tableInfo.Count; i++)
            {
                var address = GbPointerUtility.GetPointerTableItem(tableInfo.Address, i, stream);

                if (debugEnabled) w.WriteLine($"## ========== DialogueTable@{tableInfo.Address}[${i:X2}] ${address.ToAbsoluteRomAddress():X}({address}) ==========");
                w.WriteLine($"#{CommandNames.Dialogue} ${tableInfo.KeyRangeStart + i:X}");

                stream.Position = address.ToAbsoluteRomAddress();
                var decodedStringLines = NjmgStringUtility.DecodeString(stream, charmap, stringDecodeOptions).SplitLines().ToList();
                foreach (var line in decodedStringLines)
                {
                    w.WriteLine($"{linePrefix}{line}");
                }

                if (placeholderTextEnabled)
                {
                    if (tableInfo.IgnoresPlaceholderText)
                    {
                        foreach (var line in decodedStringLines)
                        {
                            w.WriteLine(line);
                        }
                    }
                    else
                    {
                        w.WriteLine($"M{tableInfo.KeyRangeStart + i:X}{charmap[ControlCodes.WaitKey]}");
                    }
                }

                w.WriteLine();
            }
        }

        for (var i = 0; i < Constants.WordTable.Count; i++)
        {
            if (debugEnabled)
            {
                var address = NjmgStringUtility.GetDictionaryStringPointer(i);
                w.WriteLine($"## ========== DictionaryItem ${i:X2} ${address.ToAbsoluteRomAddress():X}({address}) ==========");
            }
            w.WriteLine($"#{CommandNames.Word} ${i:X}");

            var lines = NjmgStringUtility.GetDictionaryString(i, charmap, stream).SplitLines().ToList();

            if (placeholderTextEnabled)
            {
                var combinedLines = string.Join("", lines);
                w.WriteLine($"{linePrefix}{combinedLines} ({{${i:X} {combinedLines}}})");
                w.WriteLine($"W{i:X}");
            }
            else
            {
                foreach (var line in lines)
                {
                    w.WriteLine($"{linePrefix}{line}");
                }
            }

            w.WriteLine();
        }

        var rawCharmap = Charmaps.JpRaw;
        w.WriteLine($"#{CommandNames.Charmap} {TextParserExtension.MakeQuotedString(Charmaps.JpRawSource)}");
        w.WriteLine();

        foreach (var tilemapInfo in Constants.TilemapInfos)
        {
            r.BaseStream.Position = tilemapInfo.Value.DestinationPointerReference.ToAbsoluteRomAddress();
            var destinationPointer = r.ReadUInt16();
            w.WriteLine($"#{CommandNames.Tilemap} ${tilemapInfo.Key:X} ${destinationPointer:X}");
            r.BaseStream.Position = tilemapInfo.Value.SourcePointerReference.ToAbsoluteRomAddress();
            var sourcePointer = r.ReadUInt16();
            r.BaseStream.Position = new GbPointer(
                tilemapInfo.Value.SourcePointerReference.Bank, sourcePointer).ToAbsoluteRomAddress();
            foreach (var row in NjmgStringUtility.DecodeTilemapString(r.BaseStream, rawCharmap))
            {
                w.WriteLine(row);
            }
            w.WriteLine();
        }

        foreach (var backgroundTilemapInfo in Constants.BackgroundTilemapInfos)
        {
            var backgroundTilemapCharmap = CompoundCharmap.FromFiles(backgroundTilemapInfo.Value.CharmapSources);
            w.WriteLine($"#{CommandNames.Charmap}{string.Join(string.Empty, backgroundTilemapInfo.Value.CharmapSources.Select(i => $" {TextParserExtension.MakeQuotedString(i)}"))}");
            w.WriteLine($"#{CommandNames.Background} ${backgroundTilemapInfo.Key:X}");
            r.BaseStream.Position = backgroundTilemapInfo.Value.SourcePointerReference.ToAbsoluteRomAddress();
            r.BaseStream.Position = new GbPointer(
                    backgroundTilemapInfo.Value.SourcePointerReference.Bank, r.ReadUInt16())
                .ToAbsoluteRomAddress();
            var backgroundTilemap = BackgroundTilemap.FromStream(stream);
            foreach (var row in backgroundTilemap.Rows)
            {
                using var memoryStream = new MemoryStream(row.ToArray());
                var decodedRow = NjmgStringUtility.DecodeRawString(memoryStream, backgroundTilemapCharmap, row.Count, warningsEnabled: false);
                w.WriteLine(decodedRow);
            }
            w.WriteLine();
        }

        w.WriteLine($"#{CommandNames.Charmap} {TextParserExtension.MakeQuotedString(Charmaps.JpSource)}");
        w.WriteLine();
        foreach (var menuTable in Constants.MenuTables)
        {
            w.WriteLine($"#{CommandNames.Menu} ${menuTable.Key:X}");
            w.WriteLine("## left up right down x y character orientation");
            stream.Position = menuTable.Value.Address.ToAbsoluteRomAddress();
            for (var i = 0; i < menuTable.Value.Count; i++)
            {
                var menuItem = new MenuItem(stream);
                if (!charmap.TryGetValue(menuItem.Character, out var mappedCharacter))
                {
                    mappedCharacter = NjmgStringUtility.MakeRawValueControlCode(menuItem.Character);
                }
                mappedCharacter = TextParserExtension.MakeQuotedString(mappedCharacter);
                w.WriteLine($"#{CommandNames.MenuItem} ${menuItem.Left:X} ${menuItem.Up:X} ${menuItem.Right:X} ${menuItem.Down:X} ${menuItem.X:X} ${menuItem.Y:X} {mappedCharacter} ${menuItem.Orientation:X} ## ${i:X}");
            }
            w.WriteLine();
        }
    }

    private static Exception CreateTokenException(
        ScriptToken token, string message, Exception? innerException = null)
    {
        return new FormatException($"Error at line {token.Line}: {message}", innerException);
    }

    public static Script LoadScript(string scriptPath)
    {
        var script = new Script();

        using var parser = new ScriptParser(PathUtility.ResolveFilePath(scriptPath));

        object? currentContentNode = null;

        var currentPalette = new List<Color>();
        var charmap = new CompoundCharmap();

        while (true)
        {
            var token = parser.ReadNext();
            if (token == null) break;

            try
            {
                if (token.Type == TokenType.Command)
                {
                    if (token.CommandName == CommandNames.Script ||
                        token.CommandName == CommandNames.Version)
                    {
                    }
                    else if (token.CommandName == CommandNames.Target)
                    {
                        var target = new ScriptTarget
                        {
                            SHA256 = token.GetParameter<string>(0),
                            Name = token.GetParameter<string>(1),
                        };
                        script.Targets.Add(target);
                    }
                    else if (token.CommandName == CommandNames.Charmap)
                    {
                        charmap = CompoundCharmap.FromFiles(
                            Enumerable.Range(0, token.Parameters.Count)
                                .Select(token.GetParameter<string>)
                                .Select(PathUtility.ResolveFilePath));
                    }
                    else if (token.CommandName == CommandNames.Dialogue)
                    {
                        var dialogueData = new DialogueData();
                        var key = token.GetParameter<int>(0);
                        var tables = Constants.DialogueTables;
                        DialogueKeyToIndices(tables, key, out _, out _);
                        script.Dialogues.Add(key, dialogueData);
                        currentContentNode = dialogueData;
                    }
                    else if (token.CommandName == CommandNames.Word)
                    {
                        var wordData = new WordData();
                        var key = token.GetParameter<int>(0);
                        if (key < 0 || key >= Constants.WordTable.Count)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        script.Words.Add(key, wordData);
                        currentContentNode = wordData;
                    }
                    else if (token.CommandName == CommandNames.Palette)
                    {
                        currentPalette.Clear();
                        for (var i = 0; i < token.Parameters.Count; i++)
                        {
                            var value = token.GetParameter<uint>(i);
                            var color = Color.FromArgb(
                                (byte)((value >> 24) & 0xFF),
                                (byte)((value >> 16) & 0xFF),
                                (byte)((value >> 08) & 0xFF),
                                (byte)((value >> 00) & 0xFF));
                            currentPalette.Add(color);
                        }
                    }
                    else if (token.CommandName == CommandNames.Image)
                    {
                        var image = new ImagePlacement
                        {
                            Offset = token.GetParameter<uint>(0),
                            Source = token.GetParameter<string>(1),
                        };

                        if (token.Parameters.Count > 2)
                        {
                            image.Bounds = new Rectangle(
                                token.GetParameter<int>(2),
                                token.GetParameter<int>(3),
                                token.GetParameter<int>(4),
                                token.GetParameter<int>(5));
                        }

                        image.Palette.AddRange(currentPalette);
                        script.Images.Add(image);
                    }
                    else if (token.CommandName == CommandNames.Tilemap)
                    {
                        var key = token.GetParameter<int>(0);
                        var tilemap = new Tilemap
                        {
                            LoadAddress = token.GetParameter<ushort>(1),
                        };
                        script.Tilemaps.Add(key, tilemap);
                        currentContentNode = tilemap;
                    }
                    else if (token.CommandName == CommandNames.Background)
                    {
                        var key = token.GetParameter<int>(0);
                        var backgroundTilemap = new BackgroundTilemap();
                        script.BackgroundTilemaps.Add(key, backgroundTilemap);
                        currentContentNode = backgroundTilemap;
                    }
                    else if (token.CommandName == CommandNames.Menu)
                    {
                        var key = token.GetParameter<int>(0);
                        var menu = new Menu();
                        script.Menus.Add(key, menu);
                        currentContentNode = menu;
                    }
                    else if (token.CommandName == CommandNames.MenuItem)
                    {
                        if (currentContentNode is not Menu currentMenu)
                            throw new IOException("Object does not support the given type of content.");

                        var menuItem = new MenuItem
                        {
                            Left = token.GetParameter<byte>(0),
                            Up = token.GetParameter<byte>(1),
                            Right = token.GetParameter<byte>(2),
                            Down = token.GetParameter<byte>(3),
                            X = token.GetParameter<byte>(4),
                            Y = token.GetParameter<byte>(5),
                        };
                        
                        var encodedCharacter = NjmgStringUtility.EncodeString(token.GetParameter<string>(6), charmap);
                        if (encodedCharacter.Length != 1)
                            throw new IOException("Invalid character string. String must map to a single byte.");
                        menuItem.Character = encodedCharacter[0];

                        menuItem.Orientation = token.GetParameter<byte>(7);

                        currentMenu.Items.Add(menuItem);
                    }
                    else if (token.CommandName == CommandNames.Patch)
                    {
                        var offset = token.GetParameter<uint>(0);
                        var dataType = token.GetParameter<DataType>(1);
                        var value = token.GetParameter(2, Patch.DataTypeToType(dataType));
                        var patch = new Patch
                        {
                            Offset = offset,
                            Value = value,
                        };
                        script.Patches.Add(patch);
                    }
                    else if (token.CommandName == CommandNames.Set)
                    {
                        var variable = token.GetParameter<string>(0);
                        if (variable == "useExpandedRom")
                        {
                            script.UseExpandedRom = token.GetParameter<bool>(1);
                        }
                        else
                        {
                            throw new IOException($"Unknown variable '{variable}'.");
                        }
                    }
                    else
                    {
                        throw new IOException($"Unknown command '#{token.CommandName}'.");
                    }
                }
                else if (token.Type == TokenType.Content)
                {
                    if (currentContentNode == null)
                        throw new FormatException("Content cannot exist out of a data item.");
                    if (currentContentNode is DialogueData dialogueData)
                    {
                        var bytes = NjmgStringUtility.EncodeString(token.Content, charmap);
                        dialogueData.Content.AddRange(bytes);
                    }
                    else if (currentContentNode is WordData wordData)
                    {
                        var bytes = NjmgStringUtility.EncodeString(token.Content, charmap);
                        if (wordData.Content.Count + bytes.Length > Constants.MaxWordLength)
                            throw new IOException("Maximum word length exceeded.");
                        wordData.Content.AddRange(bytes);
                    }
                    else if (currentContentNode is Tilemap tilemap)
                    {
                        if (tilemap.Rows.Count >= Tilemap.MaxHeight)
                            throw new IOException("Maximum tilemap height exceeded.");
                        var bytes = NjmgStringUtility.EncodeRawString(token.Content, charmap);
                        if (bytes.Length > Tilemap.MaxWidth)
                            throw new IOException("Maximum tilemap width exceeded.");
                        if (tilemap.Rows.Count > 0 && bytes.Length != tilemap.Rows[0].Count)
                            throw new IOException("Every row of a tilemap must have the same width.");
                        tilemap.Rows.Add(bytes.ToList());
                    }
                    else if (currentContentNode is BackgroundTilemap backgroundTilemap)
                    {
                        if (backgroundTilemap.Rows.Count >= backgroundTilemap.Height)
                            throw new IOException("Exceeded maximum number of tile rows.");
                        var bytes = NjmgStringUtility.EncodeRawString(token.Content, charmap);
                        if (bytes.Length != backgroundTilemap.Width)
                            throw new IOException("Row contains an incorrect number of tiles.");
                        backgroundTilemap.Rows.Add(bytes.ToList());
                    }
                    else
                    {
                        throw new IOException("Object does not support direct content.");
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw CreateTokenException(token, ex.Message, innerException: ex);
            }
        }

        Console.WriteLine($"Loaded {script.Dialogues.Count} dialogues");
        Console.WriteLine($"Loaded {script.Words.Count} words");

        return script;
    }

    private static void DialogueKeyToIndices(
        IEnumerable<TextTableInfo> tables, int key, out int tableIndex, out int index)
    {
        var i = -1;
        foreach (var table in tables)
        {
            i++;
            if (!table.ContainsKey(key)) continue;
            tableIndex = i;
            index = key - table.KeyRangeStart;
            return;
        }

        throw new ArgumentException($"No matching table found for key ${key:X}.");
    }

    private static void CheckHash(byte[] data, IEnumerable<string> hashes)
    {
        var sha256 = Convert.ToHexString(SHA256.HashData(data));
        if (!hashes.Any(i => string.Equals(sha256, i, StringComparison.InvariantCultureIgnoreCase)))
        {
            Console.WriteLine("Warning: ROM hash does not match.");
        }
    }

    private struct SectionReference
    {
        public uint Address { get; set; }
        public int Offset { get; set; }
    }

    private sealed class Section
    {
        public string Name { get; set; } = string.Empty;
        public ushort Bank { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public List<SectionReference> References { get; } = new();
    }

    private static void GetVisitedMenuItems(
        IReadOnlyList<MenuItem> menuItems, int currentIndex, ICollection<int> visitedMenuItems)
    {
        if (visitedMenuItems.Contains(currentIndex)) return;
        if (currentIndex < 0 || currentIndex >= menuItems.Count)
            throw new IndexOutOfRangeException("Menu item link index out of range.");

        visitedMenuItems.Add(currentIndex);

        var current = menuItems[currentIndex];
        var linkedItem = new[]
        {
            current.Left,
            current.Up,
            current.Right,
            current.Down,
        };

        foreach (var linkedMenuItem in linkedItem)
        {
            GetVisitedMenuItems(menuItems, linkedMenuItem, visitedMenuItems);
        }
    }

    private static List<int> GetVisitedMenuItems(IReadOnlyList<MenuItem> menuItems)
    {
        var visitedMenuItems = new List<int>();
        GetVisitedMenuItems(menuItems, 0, visitedMenuItems);
        return visitedMenuItems;
    }

    private static void GetOrMakeSection(
        List<Section> sections, ushort romBank, byte[] data,
        Func<string> sectionNameGenerator,
        out Section section, out int sectionOffset)
    {
        Section? sectionTemp = null;
        var sectionOffsetTemp = 0;

        foreach (var sectionTemp2 in sections)
        {
            if (sectionTemp2.Bank != romBank && sectionTemp2.Bank != GbPointer.CommonRomBank) continue;
            var sectionOffsetTemp2 = sectionTemp2.Data.SequenceIndexOf(data);
            if (sectionOffsetTemp2 < 0) continue;
            sectionTemp = sectionTemp2;
            sectionOffsetTemp = sectionOffsetTemp2;
        }

        if (sectionTemp == null)
        {
            sectionTemp = new Section
            {
                Name = sectionNameGenerator.Invoke(),
                Bank = romBank,
                Data = data,
            };
            sections.Add(sectionTemp);
        }

        section = sectionTemp;
        sectionOffset = sectionOffsetTemp;
    }

    public static void InjectScript(Script script, string srcRomPath, string dstRomPath)
    {
        var rom = File.ReadAllBytes(PathUtility.ResolveFilePath(srcRomPath));

        CheckHash(rom, script.Targets.Select(i => i.SHA256));

        using var stream = new MemoryStream();
        stream.Write(rom);
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);

        var freeRomSpace = new Dictionary<ushort, RangeMap>();

        const int expandedRomBanksStart = 8;
        const int expandedRomBanksEnd = 16;

        foreach (var table in Constants.DialogueTables)
        {
            var freeSpace = freeRomSpace.GetValueOrNew(table.Address.Bank);

            var dialogueIndices = new List<int>();
            for (var i = 0; i < table.Count; i++)
            {
                var key = table.KeyRangeStart + i;
                if (!script.Dialogues.TryGetValue(key, out _))
                    throw new IOException($"Missing key for dialogue ${key:X}.");
                dialogueIndices.Add(i);
            }

            foreach (var range in GetDialogueTableRangeMap(stream, table, dialogueIndices))
            {
                var pointer = GbPointer.FromAbsoluteRomAddress(range.Start);
                var pointerRange = new Range(pointer.Address, pointer.Address + range.Length);
                freeSpace.MarkRange(pointerRange);
            }
        }

        if (script.UseExpandedRom)
        {
            // De-allocate existing dialogue tables.
            foreach (var table in Constants.DialogueTables.Where(i => i.IsRelocatable))
            {
                var freeSpace = freeRomSpace.GetValueOrNew(table.Address.Bank);
                freeSpace.MarkRange(table.Address.Address, table.Address.Address + (uint)table.Count * sizeof(ushort));
            }
        }

        foreach (var (_, tilemapInfo) in Constants.TilemapInfos)
        {
            r.BaseStream.Position = tilemapInfo.SourcePointerReference.ToAbsoluteRomAddress();
            var tilemapPointer = new GbPointer(
                tilemapInfo.SourcePointerReference.Bank, r.ReadUInt16());
            r.BaseStream.Position = tilemapPointer.ToAbsoluteRomAddress();
            NjmgStringUtility.DecodeTilemapString(r.BaseStream, Charmaps.JpRaw);
            var pointerRange = new Range(tilemapPointer.Address,
                tilemapPointer.Address + checked((uint)(r.BaseStream.Position - tilemapPointer.ToAbsoluteRomAddress())));
            var freeSpace = freeRomSpace.GetValueOrNew(tilemapPointer.Bank);
            freeSpace.MarkRange(pointerRange);
        }

        foreach (var (_, backgroundTilemapInfo) in Constants.BackgroundTilemapInfos)
        {
            r.BaseStream.Position = backgroundTilemapInfo.SourcePointerReference.ToAbsoluteRomAddress();
            var tilemapPointer = new GbPointer(
                backgroundTilemapInfo.SourcePointerReference.Bank, r.ReadUInt16());
            r.BaseStream.Position = tilemapPointer.ToAbsoluteRomAddress();
            BackgroundTilemap.FromStream(stream);
            var pointerRange = new Range(tilemapPointer.Address,
                tilemapPointer.Address + checked((uint)(r.BaseStream.Position - tilemapPointer.ToAbsoluteRomAddress())));
            var freeSpace = freeRomSpace.GetValueOrNew(tilemapPointer.Bank);
            freeSpace.MarkRange(pointerRange);
        }

        foreach (var range in Constants.UnusedSpace)
        {
            var pointer = GbPointer.FromAbsoluteRomAddress(range.Start);
            var freeSpace = freeRomSpace.GetValueOrNew(pointer.Bank);
            var pointerRange = new Range(pointer.Address, pointer.Address + range.Length);
            freeSpace.MarkRange(pointerRange);
        }

        foreach (var (key, menu) in script.Menus)
        {
            if (!Constants.MenuTables.TryGetValue(key, out var tableInfo))
            {
                Console.WriteLine($"Warning: Unknown menu for key ${key:X}.");
                continue;
            }
            if (menu.Items.Count > tableInfo.Count)
                throw new Exception($"Too many menu items for menu ${key:X}.");

            if (menu.Items.Count == 0) continue;
            
            var visitedMenuItemsIndices = GetVisitedMenuItems(menu.Items);

            for (var i = 0; i < menu.Items.Count; i++)
            {
                var menuItemPointer = GbPointerUtility.GetArrayItemPointer(tableInfo.Address, i, MenuItem.SizeOf);
                if (visitedMenuItemsIndices.Contains(i))
                {
                    var menuItem = menu.Items[i];
                    stream.Position = menuItemPointer.ToAbsoluteRomAddress();
                    menuItem.Write(stream);
                }
                else
                {
                    // Free space used by unused menu items.
                    var freeSpace = freeRomSpace.GetValueOrNew(menuItemPointer.Bank);
                    var pointerRange = new Range(menuItemPointer.Address, menuItemPointer.Address + MenuItem.SizeOf);
                    freeSpace.MarkRange(pointerRange);
                }
            }
        }

        if (script.UseExpandedRom)
        {
            for (var i = expandedRomBanksStart; i < expandedRomBanksEnd; i++)
            {
                var freeSpace = freeRomSpace.GetValueOrNew((ushort)i);
                var pointerRange = new Range(GbPointer.RomXStart, GbPointer.RomXEnd);
                freeSpace.MarkRange(pointerRange);
            }
        }

        foreach (var (bank, rangeMap) in freeRomSpace)
        {
            int validRangeStart, validRangeEnd;
            if (bank == 0)
            {
                validRangeStart = GbPointer.Rom0Start;
                validRangeEnd = GbPointer.Rom0End;
            }
            else
            {
                validRangeStart = GbPointer.RomXStart;
                validRangeEnd = GbPointer.RomXEnd;
            }

            if (rangeMap.Any(range => range.Start < validRangeStart || range.End > validRangeEnd))
            {
                throw new InvalidOperationException("Free ROM space extends beyond the bounds of the ROM bank.");
            }
        }

        Console.WriteLine("Free ROM space (before injection):");
        foreach (var (romBank, map) in freeRomSpace.OrderBy(i => i.Key))
        {
            Console.WriteLine($"    Rom bank ${romBank:X}:");
            RangeMapUtility.PrintRangeMap(map, indent: "        ");
        }

        if (script.UseExpandedRom)
        {
            stream.Position = stream.Length;
            while (stream.Position < expandedRomBanksEnd * GbPointer.RomBankSize)
            {
                w.Write((byte)0xFF);
            }
        }

        var sections = new List<Section>();

        var dialogueTables = Constants.DialogueTables
            .Select(i => new TextTableInfo(i)).ToList();

        if (script.UseExpandedRom)
        {
            {
                // Split dialogue tables, when possible.

                var newDialogueTables = new List<TextTableInfo>();

                foreach (var table in dialogueTables)
                {
                    newDialogueTables.Add(table);

                    if (!table.IsSplittable || table.Count <= 1)
                        continue;

                    var splitIndex = table.Count / 2;

                    using var splitterFunctionStream = new MemoryStream();
                    using var asm = new GBZ80InstructionWriter(splitterFunctionStream, leaveOpen: true);

                    asm.ld_a(0, out var tableBankReferenceOffset);
                    asm.call(Constants.system_pushRomBank);
                    asm.ld_b(0);
                    asm.ldh_a(Constants.dialogue_nextDialogueIndex);
                    asm.sub_a(checked((byte)splitIndex));
                    asm.ld_c_a();
                    asm.sla_c();
                    asm.rl_b();
                    asm.ld_hl(0, out var tableAddressReferenceOffset);
                    asm.jp(Constants.dialogue_showDialogueAndPopRomBank);

                    if (!RangeMapUtility.TryAllocateSpace(
                            freeRomSpace, checked((uint)splitterFunctionStream.Length), 0,
                            out var splitterFunctionPointer))
                    {
                        throw new IOException("Failed to allocate splitter function.");
                    }

                    stream.Position = splitterFunctionPointer.ToAbsoluteRomAddress();
                    splitterFunctionStream.WriteTo(stream);

                    foreach (var reference in table.SplitFunctionReferences)
                    {
                        stream.Position = reference.Address;
                        if (reference.Type == ReferenceType.Address)
                        {
                            w.Write(splitterFunctionPointer.Address);
                        }
                        else if (reference.Type == ReferenceType.Bank)
                        {
                            w.Write((byte)splitterFunctionPointer.Bank);
                        }
                        else
                        {
                            throw new IndexOutOfRangeException($"Unknown reference type '{reference.Type}'.");
                        }
                    }

                    table.IsSplittable = false;
                    table.SplitFunctionReferences.Clear();

                    var splitTable = new TextTableInfo(table);
                    splitTable.KeyRangeStart += splitIndex;
                    splitTable.Count -= splitIndex;
                    splitTable.References.Clear();
                    splitTable.References.Add(
                        new Reference(
                            new GbPointer(
                                    splitterFunctionPointer.Bank,
                                    checked((ushort)(splitterFunctionPointer.Address + tableBankReferenceOffset)))
                                .ToAbsoluteRomAddress(),
                            ReferenceType.Bank));
                    splitTable.References.Add(
                        new Reference(
                            new GbPointer(
                                    splitterFunctionPointer.Bank,
                                    checked((ushort)(splitterFunctionPointer.Address + tableAddressReferenceOffset)))
                                .ToAbsoluteRomAddress(),
                            ReferenceType.Address));

                    table.Count = splitIndex;

                    newDialogueTables.Add(splitTable);
                }

                dialogueTables = newDialogueTables;
            }

            {
                // Allocate new dialogue tables in expanded ROM banks and
                // update any references to them.
                var nextRomBank = expandedRomBanksStart;

                var newDialogueTables = new List<TextTableInfo>();

                foreach (var table in dialogueTables)
                {
                    newDialogueTables.Add(table);

                    if (!table.IsRelocatable) continue;

                    if (!RangeMapUtility.TryAllocateSpace(freeRomSpace,
                            (uint)(table.Count * sizeof(ushort)),
                            (ushort)nextRomBank, out var newTableAddress))
                    {
                        throw new IOException("Failed to allocate expanded dialogue table.");
                    }

                    foreach (var reference in table.References)
                    {
                        stream.Position = reference.Address;
                        if (reference.Type == ReferenceType.Address)
                        {
                            w.Write(newTableAddress.Address);
                        }
                        else if (reference.Type == ReferenceType.Bank)
                        {
                            w.Write((byte)newTableAddress.Bank);
                        }
                        else
                        {
                            throw new IndexOutOfRangeException($"Unknown reference type '{reference.Type}'.");
                        }
                    }

                    table.Address = newTableAddress;

                    nextRomBank++;
                }

                dialogueTables = newDialogueTables;
            }
        }

        foreach (var item in script.Dialogues)
        {
            DialogueKeyToIndices(dialogueTables, item.Key, out var tableIndex, out var index);
            var table = dialogueTables[tableIndex];
            var encodedString = item.Value.Content.Concat(new byte[] { ControlCodes.End }).ToArray();
            GetOrMakeSection(sections, table.Address.Bank, encodedString, () => $"dialogue ${item.Key:X}",
                out var section, out var sectionOffset);
            var reference = new SectionReference
            {
                Address = GbPointerUtility.GetArrayItemPointer(table.Address, index, sizeof(ushort)).ToAbsoluteRomAddress(),
                Offset = sectionOffset,
            };
            section.References.Add(reference);
        }

        foreach (var tilemapInfo in Constants.TilemapInfos)
        {
            if (!script.Tilemaps.TryGetValue(tilemapInfo.Key, out var tilemap))
            {
                throw new IOException($"Missing tilemap definition for tilemap ${tilemapInfo.Key:X}.");
            }

            var bytes = tilemap.ToBytes();
            GetOrMakeSection(sections, tilemapInfo.Value.SourcePointerReference.Bank, bytes,
                () => $"tilemap ${tilemapInfo.Key:X}", out var section, out var sectionOffset);
            var reference = new SectionReference
            {
                Address = tilemapInfo.Value.SourcePointerReference.ToAbsoluteRomAddress(),
                Offset = sectionOffset,
            };
            section.References.Add(reference);

            w.BaseStream.Position = tilemapInfo.Value.DestinationPointerReference.ToAbsoluteRomAddress();
            w.Write(tilemap.LoadAddress);
        }

        foreach (var backgroundTilemapInfo in Constants.BackgroundTilemapInfos)
        {
            if (!script.BackgroundTilemaps.TryGetValue(backgroundTilemapInfo.Key, out var backgroundTilemap))
            {
                throw new IOException($"Missing background definition for background ${backgroundTilemapInfo.Key:X}.");
            }

            var bytes = backgroundTilemap.ToBytes();
            GetOrMakeSection(sections, backgroundTilemapInfo.Value.SourcePointerReference.Bank, bytes,
                () => $"background ${backgroundTilemapInfo.Key:X}", out var section, out var sectionOffset);
            var reference = new SectionReference
            {
                Address = backgroundTilemapInfo.Value.SourcePointerReference.ToAbsoluteRomAddress(),
                Offset = sectionOffset,
            };
            section.References.Add(reference);
        }

        foreach (var section in sections.OrderByDescending(i => i.Data.Length))
        {
            var allocatedSize = (uint)section.Data.Length;
            if (!RangeMapUtility.TryAllocateSpace(freeRomSpace, allocatedSize, section.Bank, out var allocatedAddress))
            {
                throw new IOException($"Failed to allocate space for section '{section.Name}'. ROM bank ${section.Bank:X} is full.");
            }
            var allocatedPointer = allocatedAddress;
            w.BaseStream.Position = allocatedPointer.ToAbsoluteRomAddress();
            w.Write(section.Data);

            foreach (var reference in section.References)
            {
                w.BaseStream.Position = reference.Address;
                w.Write((ushort)(allocatedPointer.Address + reference.Offset));
            }
        }

        for (var key = 0; key < Constants.WordTable.Count; key++)
        {
            if (!script.Words.TryGetValue(key, out var wordData))
            {
                Console.WriteLine($"Warning: Missing key for word ${key:X}.");
                continue;
            }

            stream.Position = NjmgStringUtility.GetDictionaryStringPointer(key).ToAbsoluteRomAddress();
            WriteWord(stream, wordData.Content.ToArray());
        }

        {
            if (!script.Words.TryGetValue(Constants.PlayerNameWordIndex, out var wordData))
            {
                Console.WriteLine($"Warning: Missing key for word ${Constants.PlayerNameWordIndex:X}. The default player name will not be updated.");
            }
            else
            {
                w.BaseStream.Position = Constants.DefaultPlayerName.ToAbsoluteRomAddress();
                WriteWord(stream, wordData.Content.ToArray());
            }
        }

        foreach (var imagePlacement in script.Images)
        {
            try
            {
                var image = ImageUtility.FromFile(PathUtility.ResolveFilePath(imagePlacement.Source), imagePlacement.Palette);
                if (imagePlacement.Bounds != null)
                {
                    image = image.GetSubimage(imagePlacement.Bounds.Value);
                }
                var tiles = ImageUtility.BreakIntoTiles(image, Constants.TileSize).ToList();
                r.BaseStream.Position = imagePlacement.Offset;
                foreach (var tile in tiles)
                {
                    GraphicsUtility.DrawTileToStream(tile, stream);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to place image '{imagePlacement.Source}': {ex.Message}", ex);
            }
        }

        foreach (var patch in script.Patches)
        {
            stream.Position = patch.Offset;
            Patch.WriteValue(stream, patch.Value);
        }

        RomUtility.FixRomHeader(stream);

        Console.WriteLine("Free ROM space (after injection):");
        foreach (var (romBank, map) in freeRomSpace.OrderBy(i => i.Key))
        {
            Console.WriteLine($"    Rom bank ${romBank:X}:");
            RangeMapUtility.PrintRangeMap(map, indent: "        ");
        }

        using var dstFileStream = File.Open(dstRomPath, FileMode.Create, FileAccess.Write);
        stream.WriteTo(dstFileStream);
    }

    private static void WriteWord(Stream stream, byte[] word)
    {
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);
        if (word.Length > Constants.MaxWordLength)
        {
            throw new IOException("Maximum length exceeded.");
        }
        w.Write(word);
        for (var i = word.Length; i < Constants.MaxWordLength; i++)
        {
            w.Write((byte)ControlCodes.End);
        }
    }

    private static RangeMap GetDialogueTableRangeMap(Stream rom, TextTableInfo table)
    {
        return GetDialogueTableRangeMap(rom, table, Enumerable.Range(0, table.Count));
    }

    private static RangeMap GetDialogueTableRangeMap(Stream rom, TextTableInfo table, IEnumerable<int> indices)
    {
        using var r = new BinaryReader(rom, Encoding.ASCII, true);

        var map = new RangeMap();

        var pointers = indices.Select(index => GbPointerUtility.GetPointerTableItem(table.Address, index, rom)).ToList();

        var stringDecodeOptions = NjmgStringUtility.GetTextDecodeOptions(table);

        foreach (var pointer in pointers)
        {
            r.BaseStream.Position = pointer.ToAbsoluteRomAddress();
            NjmgStringUtility.DecodeString(rom, Charmaps.Jp, stringDecodeOptions);
            map.MarkRange(pointer.ToAbsoluteRomAddress(), checked((uint)r.BaseStream.Position));
        }

        return map;
    }

    private static void LogDialogueTableRangeMap(Stream rom)
    {
        Console.WriteLine("Checking table range map...");

        for (var tableIndex = 0; tableIndex < Constants.DialogueTables.Length; tableIndex++)
        {
            var table = Constants.DialogueTables[tableIndex];
            Console.WriteLine($"    Dialogue table {tableIndex} @ ${table.Address.ToAbsoluteRomAddress():X}({table.Address}):");

            var segmentCollection = GetDialogueTableRangeMap(rom, table);

            if (segmentCollection.Ranges.Count == 0)
            {
                Console.WriteLine("        Warning: Table has zero ranges.");
            }
            else
            {
                RangeMapUtility.PrintRangeMap(segmentCollection, showGaps: true, indent: "        ");
            }
        }
    }

    private static void LogDialogueTableDuplicatePointers(Stream rom)
    {
        Console.WriteLine("Checking for duplicate dialogue table pointers...");

        using var r = new BinaryReader(rom, Encoding.ASCII, true);

        var pointers = new List<GbPointer>();

        foreach (var table in Constants.DialogueTables)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var address = GbPointerUtility.GetPointerTableItem(table.Address, i, rom);
                if (pointers.Any(j => j.Bank == address.Bank && j.Address == address.Address))
                {
                    Console.WriteLine($"    Warning: Found duplicate dialogue table pointer {address}.");
                }
                pointers.Add(address);
            }
        }
    }

    public static void LogRomInfo(string romPath)
    {
        var rom = File.ReadAllBytes(PathUtility.ResolveFilePath(romPath));
        using var stream = new MemoryStream(rom);
        LogDialogueTableRangeMap(stream);
        LogDialogueTableDuplicatePointers(stream);
    }
}
