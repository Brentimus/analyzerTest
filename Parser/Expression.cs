using Lexer;

namespace Parser;

public partial class Parser
{
    public abstract class ExpressionNode : Node
    {
        protected ExpressionNode(Lex lex = null) : base(lex)
        {
            
        }
    }
    public class UnOpExpressionNode : ExpressionNode
    {
        public UnOpExpressionNode(Lex op, ExpressionNode node)
        {
            Op = op;
            Node = node;
        }

        public Lex Op { get; }

        public ExpressionNode Node { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class BinOpExpressionNode : ExpressionNode
    {
        public BinOpExpressionNode(Lex op, Node left, Node right)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        protected Lex Op { get; set; }

        public Node Right { get; }
        public Node Left { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string ToString()
        {
            return Op.Source;
        }
    }
    
    public class WriteCallNode : CallNode
    {
        public WriteCallNode(IdNode name, List<ExpressionNode> args, bool newLine) : base(name, args)
        {
            NewLine = newLine;
        }
        public bool NewLine { get; }
    }
    public class ReadCallNode : CallNode
    {
        public ReadCallNode(IdNode name, List<ExpressionNode> args, bool newLine) : base(name, args)
        {
            NewLine = newLine;
        }
        public bool NewLine { get; }
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
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
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
        
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override string ToString()
        {
            return Field.ToString();
        }
        
    }
    public class CallNode : ExpressionNode
    {
        public CallNode(IdNode name, List<ExpressionNode> args) : base(name.LexCur)
        {
            Args = args;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        public List<ExpressionNode> Args { get; }
    }
    public class IdNode : ExpressionNode
    {
        public IdNode(Lex lexCur)
        {
            LexCur = lexCur;
        }

        public Lex LexCur { get; set; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        
        public override string ToString()
        {
            return LexCur.Value.ToString().ToLower();
        }
    }
    public class BooleanNode : ExpressionNode
    {
        public BooleanNode(Lex lexCur)
        {
            LexCur = lexCur;
        }

        protected Lex LexCur { get; }
        
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class StringNode : ExpressionNode
    {
        public StringNode(Lex lexeme)
        {
            LexCur = lexeme;
        }
        protected Lex LexCur { get; set; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
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

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected Lex LexCur { get; set; }
    }
    
    public CallNode Stream()
    {
        var lex = _curLex;
        Eat();
        Require(LexSeparator.Lparen);
        List<ExpressionNode> args = new List<ExpressionNode>();
        args = ExpressionList();
        Require(LexSeparator.Rparen);
        if (lex.Is(LexKeywords.WRITE, LexKeywords.WRITELN))
            return new WriteCallNode(new IdNode(lex), args, lex.Is(LexKeywords.WRITELN));
        return new ReadCallNode(new IdNode(lex), args, lex.Is(LexKeywords.READLN));

    }
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
            case LexType.Keyword when 
                lex.Is(LexKeywords.WRITE,LexKeywords.WRITELN):
            case LexType.Keyword when 
                lex.Is(LexKeywords.READ,LexKeywords.READLN):
                return Stream();
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
    public ExpressionNode VarRef()
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
}