using Lexer;

namespace Parser;

interface ITreePrintable
{
    public abstract void PrintTree(string branchAscii);
}

public partial class Parser
{
    private readonly Scanner _scan;
    private Lex _curLex;
    public Parser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }
    
    public abstract class Node : ITreePrintable
    {
        protected Node()
        {
        }

        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }
    
    public class KeywordNode : Node
    {
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

        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
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

        throw new Exception("Expected");
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

        throw new Exception("Expected");
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
            throw new Exception("Expect id");
        }

        return new IdNode(Eat());
    }
    public KeywordNode Keyword()
    {
        var lex = _curLex;
        if (!_curLex.Is(LexType.Keyword))
        {
            throw new Exception("Expect keyword");
        }
        Eat();
        return new KeywordNode();
    }
}