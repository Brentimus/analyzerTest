using System.Linq.Expressions;
using Lexer;
using Parser.Sym;

namespace Parser;

public partial class Parser
{
    public SymType Type()
    {
        var lex = _curLex;
        if (lex.Is(LexType.Identifier))
            return PrimitiveType();
        if (lex.Is(LexKeywords.ARRAY))
            return ArrayType();

    }

    public SymType PrimitiveType()
    {
        if (_curLex.Is(LexKeywords.STRING))
            return new SymType(Keyword().ToString()); //Maybe wrong
        return new SymType(Id().ToString());
    }

    public SymArray ArrayType()
    {
        Eat();
        Require(LexSeparator.Lbrack);
        var range = TypeRange();
        Require(LexSeparator.Rbrack);
        Require(LexKeywords.OF);
        var type = Type();
        return new SymArray(type, range);
    }

    public List<TypeRangeNode> TypeRanges()
    {
        var ranges = new List<TypeRangeNode>();
        ranges.Add(TypeRange());
        while (true)
        {
            if (!_curLex.Is(LexSeparator.Comma))
            {
                break;
            }
            Eat();
            ranges.Add(TypeRange());
        }

        return ranges;
    }

    public TypeRangeNode TypeRange()
    {
        var begin = Expression();
        Require(LexSeparator.Doubledot);
        var end = Expression();
        return new TypeRangeNode(begin, end);
    }

    public class TypeRangeNode : ExpressionNode
    {
        public TypeRangeNode(ExpressionNode begin, ExpressionNode end)
        {
            RangeTuple = new Tuple<ExpressionNode, ExpressionNode>(begin,end);
        }
        public Tuple<ExpressionNode,ExpressionNode> RangeTuple { get; set; }
        
    }

    public class FieldSelection : IdNode
    {
        public FieldSelection(Lex lexeme) : base(lexeme)
        {
        }
        
    }
    
}