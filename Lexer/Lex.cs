using System.Linq;

namespace Lexer;

public class Lex
{
    public int Column;
    public int Line;
    public LexType LexType;
    public string Source;
    public object Value;

    public Lex(LexType lexType, string source, object value, Buffer.Pos pos)
    {
        LexType = lexType;
        Value = value;
        Source = source;
        Line = pos.Line;
        Column = pos.Column;
    }

    public override string ToString()
    {
        return $"{Line}\t{Column}\t{LexType}\t{Value}\t{Source}";
    }

    public bool Is(params LexOperator[] ops)
    {
        if (LexType != LexType.Operator) return false;
        return ops.Any(op => ( Equals(op.ToString(), Value.ToString())));
    }

    public bool Is(params LexSeparator[] ops)
    {
        if (LexType != LexType.Separator) return false;
        
        return ops.Any(op => ( Equals(op.ToString(), Value.ToString())));
    }

    public bool Is(params LexKeywords[] keys)
    {
        if (LexType != LexType.Keyword) return false;
        return keys.Any(key => ( Equals(key.ToString(), Value.ToString())));
    }

    public bool Is(LexType type)
    {
        return LexType == type;
    }

    public void ConvertToId(Lex lex)
    {
        if (lex.Is(LexType.Keyword))
        {
            lex.Value = LexType.ToString().ToLower();
        }
    }
}