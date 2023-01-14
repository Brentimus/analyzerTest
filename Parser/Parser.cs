using System.Globalization;
using System.Linq.Expressions;
using Lexer;

namespace Parser;

public class Parser
{
    public abstract class Node
    {
        protected Node()
        {
        }


        public abstract void PrintTree(string branchAscii);
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

        public override void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }

    public class BinOpExpressionNode : ExpressionNode
    {
        public BinOpExpressionNode(Lex op, Node left, Node right) : base()
        {
            Op = op;
            Left = left;
            Right = right;
        }

        protected Lex Op { get; set; }

        public Node Right { get; }
        public Node Left { get; }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + Op.Source);
            branchAscii = branchAscii.Replace("├───", "│   ");
            branchAscii = branchAscii.Replace("└───", "    ");
            Left.PrintTree(branchAscii + "├───");
            Right.PrintTree(branchAscii + "└───");
        }

        public override string ToString()
        {
            return Op.Source;
        }
    }

    public abstract class DeclarationNode : Node
    {
    }

    /*public class VarDeclNode : DeclarationNode
    {
        public List<IdNode> ids { get; }
        public TypeNode type { get; }
        public ExpressionNode? exp { get; }

        public VarDeclNode(List<IdNode> ids, TypeNode type, ExpressionNode? exp)
        {
            this.ids = ids;
            this.type = type;
            this.exp = exp;
        }

        public override void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }*/

    /*public class TypeNode 
    {
        
    }*/

    /*public class VarDeclsNode : DeclarationNode
    {
        public List<VarDeclNode> dels { get; }

        public VarDeclsNode(List<VarDeclNode> dels)
        {
            this.dels = dels;
        }

        public override void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }*/

    public class StatementNode : Node
    {
        public override void PrintTree(string branchAscii)
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

        public override void PrintTree(string branchAscii)
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

        public override void PrintTree(string branchAscii)
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

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        protected Lex LexCur { get; set; }

        public override string ToString()
        {
            return LexCur.Source;
        }
    }

    public class StringNode : ExpressionNode
    {
        public StringNode(Lex lexeme)
        {
            LexCur = lexeme;
        }
        protected Lex LexCur { get; set; }
        public override void PrintTree(string branchAscii)
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

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        protected Lex LexCur { get; set; }
    }


    private readonly Scanner _scan;
    private Lex _curLex;

    public Parser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }

    /*public ProgramNode Program()
    {
        // program name;
        IdNode? name = null;
        if (_curLex.Is(LexKeywords.PROGRAM))
        {
            name = Id();
            Require(LexSeparator.Semicolom);
        }

        BlockNode block = Block();
        return new ProgramNode(name, block);
    }*/

    public class ArrayAccess : ExpressionNode
    {
        public ArrayAccess(ExpressionNode arrayId, List<ExpressionNode> arrayExp) : base()
        {
            ArrayId = arrayId;
            ArrayExp = arrayExp;
        }

        public ExpressionNode ArrayId { get; set;}
        public List<ExpressionNode> ArrayExp { get; set;}
        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + ArrayId);
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
        
        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + RecordId);
        }
    }
    public class FunctionCall : ExpressionNode
    {
        public FunctionCall() : base()
        {
        }

        public ExpressionNode ArrayId { get; set;}
        public List<ExpressionNode> ArrayExp { get; set;}
        public override void PrintTree(string branchAscii)
        {
            throw new NotImplementedException();
        }
    }

    /*public BlockNode Block()
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

    //Partical class
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
                // throw
            }

            Eat();
            //exp = Expression();
        }

        Require(LexSeparator.Semicolom);
        return new VarDeclNode(ids, type, exp);
    }*/

    public IdNode Id()
    {
        if (!_curLex.Is(LexType.Identifier))
        {
            // throw
        }

        return new IdNode(Eat());
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
            case LexType.Integer or LexType.Double:
                Eat();
                return new NumberExpressionNode(lex);
            case LexType.String:
                Eat();
                return new StringNode(lex);
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
        var left = ParseIdNode() as ExpressionNode;
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
                left = new RecordAccess(left, ParseIdNode());
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
    public List<IdNode> IdList()
    {
        var ids = new List<IdNode>();

        while (true)
        {
            ids.Add(ParseIdNode());
            Eat();
            if (!_curLex.Is(LexSeparator.Comma))
                break;
            Eat();
        }

        if (ids == null)
            throw new("null Identifier");
        
        return ids;
    }

    public IdNode ParseIdNode()
    {
        var lex = _curLex;
        Require(LexType.Identifier, false);
        return new IdNode(lex);
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
}