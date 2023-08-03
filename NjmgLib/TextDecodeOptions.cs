namespace NjmgLib;
public class TextDecodeOptions
{
    public int MaxLength { get; set; } = -1;
    public bool ExpandDictionaryWords { get; set; }
    public bool IsWindow { get; set; }
}