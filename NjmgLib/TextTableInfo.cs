namespace NjmgLib;
public class TextTableInfo : TableInfo
{
    public int KeyRangeStart { get; set; }
    public bool IsRelocatable { get; set; } = true;
    public bool IgnoresPlaceholderText { get; set; }
    public bool ContainsWindowStrings { get; set; }
    public List<Reference> References { get; } = new();

    /// <summary>
    /// Whether this <see cref="TextTableInfo"/> can be split.
    /// Note that splitting is always done down the middle.
    /// </summary>
    public bool IsSplittable { get; set; }

    /// <summary>
    /// A list of <see cref="Reference"/>s to the split function that will be used
    /// for the latter part of this <see cref="TextTableInfo"/> after being split.
    /// Only applies when <see cref="IsSplittable"/> is set.
    /// </summary>
    public List<Reference> SplitFunctionReferences { get; } = new();

    public TextTableInfo(int keyRangeStart, GbPointer address, int count) : base(address, count)
    {
        KeyRangeStart = keyRangeStart;
    }

    public TextTableInfo(TextTableInfo source) : this(source.KeyRangeStart, source.Address, source.Count)
    {
        IsRelocatable = source.IsRelocatable;
        IgnoresPlaceholderText = source.IgnoresPlaceholderText;
        ContainsWindowStrings = source.ContainsWindowStrings;
        References.AddRange(source.References);
        IsSplittable = source.IsSplittable;
        SplitFunctionReferences.AddRange(source.SplitFunctionReferences);
    }

    public bool ContainsKey(int key) =>
        key >= KeyRangeStart && key < KeyRangeStart + Count;
}
