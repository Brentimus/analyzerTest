using Lexer;

namespace Parser;

public partial class Parser
{
    public TypeNode Type()
    {
        var lex = _curLex;
        if (lex.Is(LexType.Identifier))
            return PrimitiveType();
        if (lex.Is(LexKeywords.ARRAY))
            return 
        
    }

    public TypeNode PrimitiveType()
    {
        var type = Id();
        return new PrimitiveTypeNode(type);
    }
    
    public 
    
}