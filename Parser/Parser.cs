using Lexer;

namespace Parser;

public class Parser
{
    ScannerLexer scan = new ScannerLexer();
    StreamReader _sr;
    public static Lex curLex;

    public class Node
    {
        public Node(Lex lex, Node left, Node right)
        {
            LexCur = lex;
            Left = left;
            Right = right;
        }
        public Node Right { get; set; }
        public Node Left { get; set; }
        public Lex LexCur { get; set; }
    }

    public class BinOp : Node
    {
        public BinOp(Lex lex, Node left, Node right) : base(lex, left, right)
        {
        }

        /*public int Calc(Node a)
        {
            if (LexCur.Value == (object) LexValue.Add)
                return Calc(Left) + Calc(Right);
            if (LexCur.Value == (object) LexValue.Sub)
                return Calc(Left) - Calc(Right);
            if (LexCur.Value == (object) LexValue.Mul)
                return Calc(Left) * Calc(Right);
            if (LexCur.Value == (object) LexValue.Div)
                return Calc(Left) / Calc(Right);
            return 0;
        }*/
        public override string ToString()
        {
            return LexCur.Source;
        }
        
    }
    public BinOp ParserExp(StreamReader fileReader)
    {
        _sr = fileReader;
        curLex = scan.Scanner(_sr);
        return new BinOp(curLex, null, null);
    }
    
    public Node ParseExpression()
    {
        var left = ParseTerm();
        var lex = curLex;
        while ((curLex.Value == (object) LexValue.Add) || (curLex.Value == (object) LexValue.Sub))
        {
            curLex = scan.Scanner(_sr);
            left = new BinOp(lex, left, ParseTerm());
        }
        return left;
    }
    public Node ParseTerm()
    {
        var left = ParseFactor();
        var lex = curLex;
        while ((curLex.Value == (object) LexValue.Mul) || (curLex.Value == (object) LexValue.Div))
        {
            curLex = scan.Scanner(_sr);
            left = new BinOp(lex, left, ParseTerm());
        }
        return left;
    }
    public Node ParseFactor()
    {
        var lex = curLex;
        var val = curLex;
        if (lex.LexType == LexType.Integer)
        {
            ParserExp(_sr);
            Console.WriteLine(lex);
            return new Node(lex, null, null);
        }
        if (lex.LexType == LexType.Indificator)
        {
            ParserExp(_sr);
            Console.WriteLine(lex);
            return new Node(lex, null, null);
        }

        if (lex.Value == (object) LexValue.Lparen)
        {
            ParserExp(_sr);
            var e = ParseExpression();
            if (lex.Value == (object) LexValue.Rparen)
                throw new Exception("no Rparen");
            ParserExp(_sr);
            return e;
        }
        throw new Exception("Factor expected");
    }
}