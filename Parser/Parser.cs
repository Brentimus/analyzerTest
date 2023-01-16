using System.Globalization;
using System.Linq.Expressions;
using Lexer;
using Parser.Sym;

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
    

    public abstract class ExpressionNode : Node
    {
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

        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + Op.Source);
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

        public void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + Op.Source);
            branchAscii = branchAscii.Replace("├───", "│   ");
            branchAscii = branchAscii.Replace("└───", "    ");
            Left.PrintTree(branchAscii + "├───");
            Right.PrintTree(branchAscii + "└───");
        }

        public string ToString()
        {
            return Op.Source;
        }
    }

    public abstract class DeclarationNode : Node
    {
    }

    public class VarDeclNode : DeclarationNode
    {
        public List<IdNode> ids { get; }
        public SymType type { get; }
        public ExpressionNode? exp { get; }

        public VarDeclNode(List<IdNode> ids, SymType type, ExpressionNode? exp)
        {
            this.ids = ids;
            this.type = type;
            this.exp = exp;
        }
    }

    public class VarDeclsNode : DeclarationNode
    {
        public List<VarDeclNode> dels { get; }

        public VarDeclsNode(List<VarDeclNode> dels)
        {
            this.dels = dels;
        }

        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }

    public class StatementNode : Node
    {
        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }

    public class KeywordNode : Node
    {
        public KeywordNode()
        {
        }

        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }

    public class CompoundStatementNode : StatementNode
    {
    }

    public class BlockNode : Node
    {
        public List<DeclarationNode> Declarations { get; }
        public CompoundStatementNode Compound { get; }

        public BlockNode(List<DeclarationNode> declarations, CompoundStatementNode compound)
        {
            Declarations = declarations;
            Compound = compound;
        }

        public void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
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

    public BlockNode Block()
    {
        var declarations = new List<DeclarationNode>();
        while (true)
        {
            if (_curLex.Is(LexKeywords.VAR))
            {
                Eat();
                declarations.Add(VarDecls());
            }
            else if (_curLex.Is(LexKeywords.TYPE))
            {
            }
            else if (_curLex.Is(LexKeywords.CONST))
            {
            }
            else if (_curLex.Is(LexKeywords.PROCEDURE))
            {
            }
            else if (_curLex.Is(LexKeywords.FUNCTION))
            {
            }
            else
            {
                break;
            }
        }

        //var compound = CompoundStatement();
        return new BlockNode(declarations, compound);
    }
    public VarDeclsNode VarDecls()
    {
        var dels = new List<VarDeclNode>();
        dels.Add(VarDecl());
        while (_curLex.Is(LexType.Identifier))
        {
            dels.Add(VarDecl());
        }

        return new VarDeclsNode(dels);
    }

    public VarDeclNode VarDecl()
    {
        var ids = new List<IdNode>();
        ids.Add(Id());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            ids.Add(Id());
        }
        Require(LexSeparator.Colon);
        var type = Type();
        ExpressionNode? exp = null;
        if (_curLex.Is(LexOperator.Equal))
        {
            if (ids.Count > 1)
            {
                throw new Exception("Error initialization");
            }
            Eat();
            exp = Expression();
        }

        Require(LexSeparator.Semicolom);
        return new VarDeclNode(ids, type, exp);
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
        Eat();
        return new KeywordNode();
    }
}