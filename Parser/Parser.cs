using Lexer;

namespace Parser;
public class Parser
{
    private Lex _curLex;
    private readonly ScannerLexer _scan;
    public Parser(StreamReader fileReader)
    {
        _scan = new ScannerLexer(fileReader);
        _curLex = _scan.Scanner();
    }
    public abstract class Node
    {
        protected Node(Lex lexeme)
        {
            LexCur = lexeme;
        }
        protected Lex LexCur { get; set; }

        public abstract void PrintTree(string treeString = "");

        public abstract double Calc();
    }
    private class BinOp : Node
    {
        public BinOp(Lex lexCur, Node left, Node right) : base(lexCur)
        {
            Left = left;
            Right = right;
        }

        private Node Right { get; set; }
        private Node Left { get; set; }
        
        public override void PrintTree(string treeString = "")
        {
            Console.WriteLine($"{treeString}{LexCur.Source}");
            treeString = treeString.Replace("├───", "│   ");
            treeString = treeString.Replace("└───", "    ");
            Left.PrintTree(treeString + "├───");
            Right.PrintTree(treeString+ "└───");
        }
        public override double Calc()
        {
            switch ((LexValue)LexCur.Value)
            {
                case LexValue.Add:
                    return Left.Calc() + Right.Calc();
                case LexValue.Sub:
                    return Left.Calc() - Right.Calc();
                case LexValue.Mul:
                    return Left.Calc() * Right.Calc();
                case LexValue.Div:
                    return Left.Calc() / Right.Calc();
            }
            throw new Exception("error calc");
        }
        public override string ToString()
        {
            return LexCur.Source;
        }
        
    }
    class IdNode : Node
    {
        public IdNode(Lex lexeme) : base(lexeme) {}

        public override void PrintTree(string treeString = "")
        {
            Console.WriteLine($"{treeString}{LexCur.Source}");
        }

        public override double Calc()
        {
            throw new NotImplementedException(); // Пока переменные не инициализируется
        }
    }
    private class NumberNode : Node
    { 
        public NumberNode(Lex lexeme) : base(lexeme) {}
        public override string ToString()
        {
            return LexCur.Source;
        }
        
        public override void PrintTree(string treeString = "")
        {
            Console.WriteLine($"{treeString}{LexCur.Source}");
        }
        
        public override double Calc()
        {
            return double.Parse(LexCur.Source);
        }
    }
    public Node ParseExpression()
    {
        var left = ParseTerm();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator && (lex.Value.ToString() == LexValue.Add.ToString() || lex.Value.ToString() == LexValue.Sub.ToString()))
        {
            _curLex = _scan.Scanner();
            left = new BinOp(lex, left, ParseTerm());
            lex = _curLex;
        }
        return left;
    }
    private Node ParseTerm()
    {
        var left = ParseFactor();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator && (lex.Value.ToString() == LexValue.Mul.ToString()) || (lex.Value.ToString() == LexValue.Div.ToString()))
        {
            _curLex = _scan.Scanner();
            left = new BinOp(lex, left, ParseFactor());
            lex = _curLex;
        }
        return left;
    }
    private Node ParseFactor()
    {
        var lex = _curLex;
        
        if (lex.LexType == LexType.Integer || lex.LexType == LexType.Double)
        {
            _curLex = _scan.Scanner();
            return new NumberNode(lex);
        }
        if (lex.LexType == LexType.Indificator)
        {
            _curLex = _scan.Scanner();
            return new IdNode(lex);
        }

        if (lex.Value.ToString() == LexValue.Lparen.ToString())
        {
            _curLex = _scan.Scanner();
            var e = ParseExpression();
            if (_curLex.Value.ToString() != LexValue.Rparen.ToString())
                throw new Exception(_curLex.Line +":"+ _curLex.Column + " no Rparen");
            _curLex = _scan.Scanner();
            return e;
        }
        throw new Exception(_curLex.Line +":"+ _curLex.Column + " Factor expected");
    }
}