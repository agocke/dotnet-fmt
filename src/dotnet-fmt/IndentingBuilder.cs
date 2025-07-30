
namespace DotnetFmt;

internal struct IndentingBuilder
{
    private readonly StringBuilder _sb;
    private int _indentLevel;

    public IndentingBuilder(StringBuilder sb)
    {
        _sb = sb;
        _indentLevel = 0;
    }

    public void Indent() => _indentLevel++;
    public void Dedent() => _indentLevel--;

    public void Append(string s)
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append("    "); // 4 spaces for indentation
        }

    private void Append(string s)
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append(Indent);
        }
        _sb.Append(s);
    }

    private void AppendLine()
    {
        _sb.AppendLine();
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append(Indent);
        }
    }

    private void AppendLine(string s)
    {
        Append(s);
        _sb.AppendLine();
    }