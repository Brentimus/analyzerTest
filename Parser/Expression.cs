using Lexer;

namespace Parser;

public partial class Parser
{
    public ExpressionNode Expression()
    {
        var left = SimpleExpression();
        var lex = _curLex;
        while (lex.Is(LexOperator.Equal, LexOperator.Less, LexOperator.More, LexOperator.NoEqual, LexOperator.LessEqual,
                   LexOperator.MoreEqual))
        {
            Eat();
            left = new BinOpExpressionNode(lex, left, SimpleExpression());
            lex = _curLex;
        }

        return left;
    }
    private ExpressionNode SimpleExpression()
    {
        var left = Term();
        var lex = _curLex;
        while (lex.Is(LexOperator.Add, LexOperator.Sub) || lex.Is(LexKeywords.OR, LexKeywords.XOR))
        {
            Eat();
            left = new BinOpExpressionNode(lex, left, Term());
            lex = _curLex;
        }

        return left;
    }
    private ExpressionNode Term()
    {
        var left = SimpleTerm();
        var lex = _curLex;
        while (lex.Is(LexOperator.Mul, LexOperator.Div) || lex.Is(LexKeywords.MOD, LexKeywords.AND, LexKeywords.SHR,
                   LexKeywords.SHL, LexKeywords.DIV))
        {
            Eat();
            left = new BinOpExpressionNode(lex, left, SimpleTerm());
            lex = _curLex;
        }

        return left;
    }
    private ExpressionNode SimpleTerm()
    {
        if (!_curLex.Is(LexOperator.Add, LexOperator.Sub) && !_curLex.Is(LexKeywords.NOT)) return Factor();
        var op = _curLex;
        Eat();
        return new UnOpExpressionNode(op, SimpleTerm());
    }
    private ExpressionNode Factor()
    {
        var lex = _curLex;

        switch (lex.LexType)
        {
            case LexType.Integer or LexType.Double:
                Eat();
                return new NumberExpressionNode(lex);
            case LexType.String:
                Eat();
                return new StringNode(lex);
            case LexType.Identifier:
                return VarRef();
            case LexType.Keyword:
                if (lex.Is(LexKeywords.TRUE, LexKeywords.FALSE))
                    return new BooleanNode(lex);
                break;
        }

        if (lex.Is(LexSeparator.Lparen))
            throw new Exception(_curLex.Line + ":" + _curLex.Column + " Factor expected");
        Eat();
        var e = Expression();

        if (lex.Is(LexSeparator.Rparen))
            throw new Exception(_curLex.Line + ":" + _curLex.Column + " no Rparen");
        Eat();
        return e;
    }
    private ExpressionNode VarRef()
    {
        var left = Id() as ExpressionNode;
        var lex = _curLex;
        while (true)
        {
            if (lex.Is(LexSeparator.Lbrack))
            {
                //ArrayAccess
                Eat();
                List<ExpressionNode> args = new List<ExpressionNode>();
                if (!lex.Is(LexSeparator.Rparen))
                {
                    args = ExpressionList();
                }
                left = new ArrayAccess(left, args);
                Require(LexSeparator.Rparen);
                lex = _curLex;
            }
            else if (lex.Is(LexSeparator.Dot))
            { 
                //RecordAccess
                Eat();
                left = new RecordAccess(left, Id());
                lex = _curLex;
            }
            else if (lex.Is(LexSeparator.Lparen))
            {
                //FunctionCall
                Eat();
                List<ExpressionNode> args = new List<ExpressionNode>();
                
                if (!lex.Is(LexSeparator.Rparen))
                {
                    args = ExpressionList();
                }
                //left = new FunctionCall();
                
                Require(LexSeparator.Rparen);
                
                lex = _curLex;
            }
            else
            {
                return left;
            }
        }
    }
    public List<ExpressionNode> ExpressionList()
    {
        var exps = new List<ExpressionNode>();

        while (true)
        {
            exps.Add(Expression());
            if (!_curLex.Is(LexSeparator.Comma))
                break;
            Eat();
        }

        if (exps == null)
            throw new("null expression");
        
        return exps;
    }
    public class ArrayAccess : ExpressionNode
    {
        public ArrayAccess(ExpressionNode arrayId, List<ExpressionNode> arrayExp) : base()
        {
            ArrayId = arrayId;
            ArrayExp = arrayExp;
        }

        public ExpressionNode ArrayId { get; set;}
        public List<ExpressionNode> ArrayExp { get; set;}
        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + ArrayId + ArrayExp);
        }

        public override string ToString()
        {
            return ArrayId +" "+ArrayExp;
        }
    }
    public class RecordAccess : ExpressionNode
    {
        public RecordAccess(ExpressionNode recordId, IdNode field)
        {
            RecordId = recordId;
            Field = field;
        }
        public ExpressionNode RecordId { get; set; }
        public IdNode Field { get; set; }
        
        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + RecordId);
            branchAscii = branchAscii.Replace("├───", "│   ");
            branchAscii = branchAscii.Replace("└───", "    ");
            RecordId.PrintTree(branchAscii + "└───");
        }

        public override string ToString()
        {
            return Field.ToString();
        }
    }
    public class FunctionCall : ExpressionNode
    {
        public FunctionCall() : base()
        {
        }

        public ExpressionNode ArrayId { get; set;}
        public List<ExpressionNode> ArrayExp { get; set;}
        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }
    
    public class IdNode : ExpressionNode
    {
        public IdNode(Lex lexeme) : base()
        {
            LexCur = lexeme;
        }

        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        public Lex LexCur { get; set; }

        public override string ToString()
        {
            return LexCur.Source;
        }
    }
    public class BooleanNode : ExpressionNode
    {
        public BooleanNode(Lex lexCur)
        {
            LexCur = lexCur;
        }

        protected Lex LexCur { get; }
        
        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }
    public class StringNode : ExpressionNode
    {
        public StringNode(Lex lexeme)
        {
            LexCur = lexeme;
        }
        protected Lex LexCur { get; set; }
        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }
        public override string ToString()
        {
            return LexCur.Source;
        }
    }
    public class NumberExpressionNode : ExpressionNode
    {
        public NumberExpressionNode(Lex lexeme) : base()
        {
            LexCur = lexeme;
        }

        public override string ToString()
        {
            return LexCur.Source;
        }

        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        protected Lex LexCur { get; set; }
    }
    public List<IdNode> IdList()
    {
        var ids = new List<IdNode>();

        while (true)
        {
            ids.Add(Id());
            Eat();
            if (!_curLex.Is(LexSeparator.Comma))
                break;
            Eat();
        }

        if (ids == null)
            throw new("null Identifier");
        
        return ids;
    }
}