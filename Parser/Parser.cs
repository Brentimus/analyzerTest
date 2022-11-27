using System.Text;
using Lexer;
using Microsoft.VisualBasic;

namespace Parser;

public class Parser
{
    private Lex curLex;
    private ScannerLexer scan;
    public Parser(StreamReader fileReader)
    {
        scan = new ScannerLexer(fileReader);
        curLex = scan.Scanner();
    }

    public class Node
    {
        public Node(Lex lexeme)
        {
            LexCur = lexeme;
        }
        public Lex LexCur { get; set; }

        virtual public void PrintTree(string indent="", string side = "")
        {
            indent = indent.Replace("├───", "│   ");
            indent = indent.Replace("└───", "    ");
            if (side == "R")
                indent += "└───";
            else if (side == "L")
                indent += "├───";
            Console.WriteLine($"{indent}{LexCur.Source}");
        }

        virtual public double Calc()
        {
            double num;
            if (double.TryParse(LexCur.Source, out num))
            {
                return num;
            }
            //Пока переменные не инициализируется
            throw new Exception(LexCur.Source+ "\t not number");
        }
    }

    public class BinOp : Node
    {
        public BinOp(Lex lexCur, Node left, Node right) : base(lexCur)
        {
            Left = left;
            Right = right;
        }
        public Node Right { get; set; }
        public Node Left { get; set; }
        
        public override void PrintTree(string indent, string side)
        {
            indent = indent.Replace("├───", "│   ");
            indent = indent.Replace("└───", "    ");
            if (LexCur != null)
            {
                if (side == "R")
                    indent += "└───";
                else if (side == "L")
                    indent += "├───";
                Console.WriteLine($"{indent}{LexCur.Source}");
                
                Left.PrintTree(indent, "L");
                Right.PrintTree(indent, "R");
            }
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
    public class IdNode : Node
    {
        public IdNode(Lex lexeme) : base(lexeme) {}
    }
    public class NumberNode : Node
    { 
        public NumberNode(Lex lexeme) : base(lexeme) {}
        public override string ToString()
        {
            return LexCur.Source;
        }
    } 
    
    public Node ParseExpression()
    {
        var left = ParseTerm();
        var lex = curLex;
        while (lex.LexType == LexType.Operator && (lex.Value.ToString() == LexValue.Add.ToString() || lex.Value.ToString() == LexValue.Sub.ToString()))
        {
            curLex = scan.Scanner();
            left = new BinOp(lex, left, ParseTerm());
            lex = curLex;
        }
        return left;
    }
    public Node ParseTerm()
    {
        var left = ParseFactor();
        var lex = curLex;
        while (lex.LexType == LexType.Operator && (lex.Value.ToString() == LexValue.Mul.ToString()) || (lex.Value.ToString() == LexValue.Div.ToString()))
        {
            curLex = scan.Scanner();
            left = new BinOp(lex, left, ParseFactor());
            lex = curLex;
        }
        return left;
    }
    public Node ParseFactor()
    {
        var lex = curLex;
        
        if (lex.LexType == LexType.Integer || lex.LexType == LexType.Double)
        {
            curLex = scan.Scanner();
            return new NumberNode(lex);
        }
        if (lex.LexType == LexType.Indificator)
        {
            curLex = scan.Scanner();
            return new IdNode(lex);
        }

        if (lex.Value.ToString() == LexValue.Lparen.ToString())
        {
            curLex = scan.Scanner();
            var e = ParseExpression();
            if (curLex.Value.ToString() != LexValue.Rparen.ToString())
                throw new Exception(curLex.Line +":"+ curLex.Column + " no Rparen");
            curLex = scan.Scanner();
            return e;
        }
        throw new Exception("Factor expected");
    }
}