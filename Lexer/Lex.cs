namespace Lexer;

public class Lex
{
    public int Column;
    public LexType LexType;
    public int Line;
    public string Source;
    public object Value;

    public Lex(LexType lexType, string source, object value, int line, int column)
    {
        LexType = lexType;
        Value = value;
        Source = source;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        if (LexType == LexType.Eof)
            return Line + "  " + Column + "  " + LexType;
        return Line + "  " + Column + "  " + LexType + "  " + Value + "  " + Source;
    }
}