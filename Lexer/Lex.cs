using System;
using System.Linq;

namespace Lexer;

public class Lex
{
    public Buffer.Pos Pos;
    public LexType LexType;
    public string Source;
    public object Value;

    public Lex(LexType lexType, string source, object value, Buffer.Pos pos)
    {
        LexType = lexType;
        Value = value;
        Source = source;
        Pos = pos;
    }

    public override string ToString()
    {
        return $"{Pos.Line}\t{Pos.Column}\t{LexType}\t{Value}\t{Source}";
    }

    public bool Is(params LexOperator[] ops)
    {
        if (LexType != LexType.Operator) return false;
        return ops.Any(op => op == (LexOperator) Value);
    }

    public bool Is(params LexSeparator[] ops)
    {
        if (LexType != LexType.Separator) return false;
        
        return ops.Any(op => op == (LexSeparator) Value);
    }
    public bool Is(params LexKeywords[] keys)
    {
        if (LexType != LexType.Keyword) return false;
        return keys.Any(key =>  key == (LexKeywords) Enum.Parse(typeof(LexKeywords), Value.ToString()!));
    }
    public bool Is(params LexType[] types)
    {
        return types.Any(type =>  type == LexType);
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