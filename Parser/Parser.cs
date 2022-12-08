using Lexer;

namespace Parser;

public class Parser
{
    private readonly Scanner _scan;
    private Lex _curLex;

    public Parser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }

    public Node ParseExpression()
    {
        var left = ParseTerm();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator &&
               (Equals(lex.Value, LexToken.Add) || Equals(lex.Value, LexToken.Sub)))
        {
            _curLex = _scan.ScannerLex();
            left = new BinOp(lex, left, ParseTerm());
            lex = _curLex;
        }

        return left;
    }

    private Node ParseTerm()
    {
        var left = ParseFactor();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator &&
               (Equals(lex.Value, LexToken.Mul) || Equals(lex.Value, LexToken.Div)))
        {
            _curLex = _scan.ScannerLex();
            left = new BinOp(lex, left, ParseFactor());
            lex = _curLex;
        }

        return left;
    }

    private Node ParseFactor()
    {
        var lex = _curLex;

        switch (lex.LexType)
        {
            case LexType.Integer or LexType.Double:
                _curLex = _scan.ScannerLex();
                return new NumberNode(lex);
            case LexType.Identifier:
                _curLex = _scan.ScannerLex();
                return new IdNode(lex);
        }

        if (!Equals(lex.Value, LexToken.Lparen))
            throw new Exception(_curLex.Line + ":" + _curLex.Column + " Factor expected");
        _curLex = _scan.ScannerLex();
        var e = ParseExpression();

        if (!Equals(_curLex.Value, LexToken.Rparen))
            throw new Exception(_curLex.Line + ":" + _curLex.Column + " no Rparen");
        _curLex = _scan.ScannerLex();
        return e;
    }

    public abstract class Node
    {
        protected Node(Lex lexeme)
        {
            LexCur = lexeme;
        }

        protected Lex LexCur { get; set; }

        public abstract void PrintTree(string branchAscii);

        public abstract double Calc();
    }

    private class BinOp : Node
    {
        public BinOp(Lex lexCur, Node left, Node right) : base(lexCur)
        {
            Left = left;
            Right = right;
        }

        private Node Right { get; }
        private Node Left { get; }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
            branchAscii = branchAscii.Replace("├───", "│   ");
            branchAscii = branchAscii.Replace("└───", "    ");
            Left.PrintTree(branchAscii + "├───");
            Right.PrintTree(branchAscii + "└───");
        }

        public override double Calc()
        {
            return (LexToken) LexCur.Value switch
            {
                LexToken.Add => Left.Calc() + Right.Calc(),
                LexToken.Sub => Left.Calc() - Right.Calc(),
                LexToken.Mul => Left.Calc() * Right.Calc(),
                LexToken.Div => Left.Calc() / Right.Calc(),
                _ => throw new Exception("error calc")
            };
        }

        public override string ToString()
        {
            return LexCur.Source;
        }
    }

    private class IdNode : Node
    {
        public IdNode(Lex lexeme) : base(lexeme)
        {
        }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        public override string ToString()
        {
            return LexCur.Source;
        }

        public override double Calc()
        {
            throw new NotImplementedException(); // Пока переменные не инициализируется
        }
    }

    private class NumberNode : Node
    {
        public NumberNode(Lex lexeme) : base(lexeme)
        {
        }

        public override string ToString()
        {
            return LexCur.Source;
        }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        public override double Calc()
        {
            return double.Parse(LexCur.Source);
        }
    }
}