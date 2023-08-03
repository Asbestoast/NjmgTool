namespace NjmgLib;

public class TableInfo
{
    public GbPointer Address { get; set; }
    public int Count { get; set; }

    public TableInfo(GbPointer address, int count)
    {
        Address = address;
        Count = count;
    }
}