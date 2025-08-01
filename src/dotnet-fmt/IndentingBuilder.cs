
using System.Text;

namespace DotnetFmt;

internal struct IndentingBuilder
{
    private readonly StringBuilder _sb;
    private int _indentLevel;

    public IndentingBuilder()
    {
        _sb = new StringBuilder();
        _indentLevel = 0;
    }

    public void Indent() => _indentLevel++;
    public void Dedent() => _indentLevel--;

    public void AppendRaw(string s)
    {
        _sb.Append(s);
    }

    public void Append(string s)
    {
        StartLine();
        _sb.Append(s);
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    public void AppendLine(string s)
    {
        Append(s);
        _sb.AppendLine();
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    internal void StartLine()
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append("    "); // 4 spaces for indentation
        }
    }
}