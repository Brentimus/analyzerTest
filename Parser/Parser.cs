using System.Data;
using Lexer;

namespace Parser;

public partial class Parser : Buffer
{
    private readonly Scanner _scan;
    private Lex _curLex;

    public Parser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }

    public abstract class Node
    {
        protected Node(Lex lex = null) => Lex = lex;
        public Lex Lex { get; set; }

        public abstract void Accept(IVisitor visitor);
        
    }

    public class KeywordNode : Node
    {
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ProgramNode : Node
    {
        public ProgramNode(IdNode? name, BlockNode block)
        {
            Name = name;
            Block = block;
        }

        public IdNode? Name { get; }
        public BlockNode Block { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public ProgramNode Program()
    {
        // program name;
        IdNode? name = null;
        if (_curLex.Is(LexKeywords.PROGRAM))
        {
            Eat();
            name = Id();
            Require(LexSeparator.Semicolom);
        }

        BlockNode block = Block();
        Require(LexSeparator.Dot);
        return new ProgramNode(name, block);
    }

    public void Require(LexOperator op, bool eat = true)
    {
        if (_curLex.Is(op))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new Exception("Expected");
    }

    public void Require(LexType op, bool eat = true)
    {
        if (_curLex.Is(op))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new Exception("Expected");
    }

    public void Require(LexSeparator sep, bool eat = true)
    {
        if (_curLex.Is(sep))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new SyntaxException(_curLex.Line, _curLex.Column, $"Expected '{sep}'");
    }

    public void Require(LexKeywords keyword, bool eat = true)
    {
        if (_curLex.Is(keyword))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new SyntaxException(_curLex.Line, _curLex.Column, $"Expected '{keyword}'");
    }

    public Lex Eat()
    {
        _curLex = _scan.ScannerLex();
        return _curLex;
    }

    public IdNode Id()
    {
        if (!_curLex.Is(LexType.Identifier))
        {
            throw new SyntaxException(_curLex.Line, _curLex.Column, "Expect Identifier");
        }

        var lex = _curLex;
        Eat();
        return new IdNode(lex);
    }

    public KeywordNode Keyword()
    {
        var lex = _curLex;
        if (!_curLex.Is(LexType.Keyword))
        {
            throw new Exception("Expect Keyword");
        }

        Eat();
        return new KeywordNode();
    }
}