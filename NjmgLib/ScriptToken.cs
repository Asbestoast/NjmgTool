namespace NjmgLib;
internal class ScriptToken
{
    public TokenType Type { get; set; }
    public string CommandName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<object> Parameters { get; } = new();
    public int Line { get; set; }

    public T GetParameter<T>(int index) => (T)GetParameter(index, typeof(T));

    public object GetParameter(int index, Type type)
    {
        if (type.IsEnum && Parameters[index] is string)
        {
            return Enum.Parse(type, (string)Convert.ChangeType(Parameters[index], typeof(string)));
        }
        return Convert.ChangeType(Parameters[index], type);
    }
}
