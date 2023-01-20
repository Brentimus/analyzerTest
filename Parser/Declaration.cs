using System.Reflection.Metadata;
using Lexer;
using Parser.Sym;

namespace Parser;

public partial class Parser
{
    public abstract class DeclarationNode : Node
    {
    }
    public class BlockNode : Node
    {
        public BlockNode(List<IAcceptable> declarations, CompoundStatementNode compound)
        {
            Declarations = declarations;
            Compound = compound;
        }

        public List<IAcceptable> Declarations { get; }
        public CompoundStatementNode Compound { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    /*public class ProcDelcNode : DeclarationNode
    {
        public ProcDelcNode(SymProcedure procedure)
        {
            Procedure = procedure;
        }
        public SymProcedure Procedure { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        
    }
    public class FuncDelcNode : DeclarationNode
    {
        public FuncDelcNode(SymFunction function)
        {
            Function = function;
        }
        public IdNode Id { get; }
        public List<Parameter>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }*/
    public class VarDeclNode : DeclarationNode
    {
        public VarDeclNode(List<IdNode> names, List<SymVarParam> symVarParams, ExpressionNode? exp)
        {
            this.names = names;
            SymVarParams = symVarParams;
            Exp = exp;
        }

        public List<IdNode> names { get; }
        public List<SymVarParam> SymVarParams { get; }
        public ExpressionNode? Exp { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class VarDeclsNode : DeclarationNode
    {
        public List<VarDeclNode> Dels { get; }

        public VarDeclsNode(List<VarDeclNode> dels)
        {
            Dels = dels;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class ConstDeclsNode : DeclarationNode
    {
        public ConstDeclsNode(List<ConstDeclNode> decls)
        {
            Decls = decls;
        }
        public List<ConstDeclNode> Decls { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        
    }
    public class ConstDeclNode : DeclarationNode
    {
        public ConstDeclNode(SymConst symConst, ExpressionNode exp)
        {
            SymConst = symConst;
            Exp = exp;
        }
        public SymConst SymConst { get; }
        public ExpressionNode Exp { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class TypeDeclsNode : DeclarationNode
    {
        public TypeDeclsNode(List<SymAlias> typeDecs)
        {
            TypeDecs = typeDecs;
        }

        public List<SymAlias> TypeDecs { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class ParamSelectionNode : DeclarationNode
    {
        public ParamSelectionNode(KeywordNode modifier, List<IdNode> ids, SymType type)
        {
            Modifier = modifier;
            Ids = ids;
            Type = type;
        }

        public KeywordNode Modifier { get; }
        public List<IdNode> Ids { get; }
        public SymType Type { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        
    }
    public ParamSelectionNode Parameter()
    {
        KeywordNode modifier = null!;
        if (_curLex.Is(LexKeywords.VAR, LexKeywords.CONST))
        {
            modifier = Keyword();
        }
        var idList = IdList();
        Require(LexSeparator.Colon);
        return new ParamSelectionNode(modifier, idList, Type());
    }
    public SymFunction FuncDecl()
    {
        Eat();
        IdNode id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParamSelectionNode>();
        if (!_curLex.Is(LexSeparator.Rparen))
        {
            locals.Add(Parameter());
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                locals.Add(Parameter());
            }
        }
        Require(LexSeparator.Rparen);
        Require(LexSeparator.Colon);
        var type = Type();
        Require(LexSeparator.Semicolom);
        var decls = CallBlock();
        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        {
            foreach (var idNode in local.Ids)
            {
                table.Push(new SymVar(idNode, local.Type), true);
            }
        }
        return new SymFunction(id, table, new BlockNode(decls, compound), type);
    }
    public SymProcedure ProcDecl()
    {
        Eat();
        IdNode id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParamSelectionNode>();
        if (!_curLex.Is(LexSeparator.Rparen))
        {
            locals.Add(Parameter());
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                locals.Add(Parameter());
            }
        }
        Require(LexSeparator.Rparen);
        Require(LexSeparator.Semicolom);
        var decls = CallBlock();
        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        {
            foreach (var idNode in local.Ids)
            {
                table.Push(new SymVar(idNode, local.Type), true);
            }
        }

        return new SymProcedure(id, table, new BlockNode(decls, compound));
    }

    public List<IAcceptable> CallBlock()
    {
        var declarations = new List<IAcceptable>();
        while (true)
        {
            if (_curLex.Is(LexKeywords.VAR))
            {
                declarations.Add(VarDecls());
            }
            else if (_curLex.Is(LexKeywords.TYPE))
            {
                declarations.Add(TypeDecls());
            }
            else if (_curLex.Is(LexKeywords.CONST))
            {
                declarations.Add(ConstDecls());
            }else
            {
                break;
            }
        }
        return declarations;
    }
    public BlockNode Block()
    {
        var declarations = new List<IAcceptable>();
        while (true)
        {
            if (_curLex.Is(LexKeywords.VAR))
            {
                declarations.Add(VarDecls());
            }
            else if (_curLex.Is(LexKeywords.TYPE))
            {
                declarations.Add(TypeDecls());
            }
            else if (_curLex.Is(LexKeywords.CONST))
            {
                declarations.Add(ConstDecls());
            }
            else if (_curLex.Is(LexKeywords.PROCEDURE))
            {
                declarations.Add(ProcDecl());
            }
            else if (_curLex.Is(LexKeywords.FUNCTION))
            {
                declarations.Add(FuncDecl());
            }
            else
            {
                break;
            }
        }
        var compound = CompoundStatement();
        return new BlockNode(declarations, compound);
    }
    public ConstDeclsNode ConstDecls()
    {
        Eat();
        var decls = new List<ConstDeclNode>();
        decls.Add(ConstDecl());
        while (_curLex.Is(LexType.Identifier))
        {
            decls.Add(ConstDecl());
        }

        return new ConstDeclsNode(decls);
    }
    public ConstDeclNode ConstDecl()
    {
        var id = Id();
        SymType type = null;
        if (_curLex.Is(LexSeparator.Colon))
        {
            Eat();
            type = Type();
        }
        Require(LexOperator.Equal);
        var exp = Expression();
        Require(LexSeparator.Semicolom);
        if (type == null)
        {
            return new ConstDeclNode(new SymConst(id, null), exp);
        }

        return new ConstDeclNode(new SymConst(id, type), exp);
    }
    public SymAlias TypeDecl()
    {
        var id = Id();
        Require(LexOperator.Equal);
        var type = Type();
        Require(LexSeparator.Semicolom);
        return new SymAlias(id, type);
    }
    public TypeDeclsNode TypeDecls()
    {
        Eat();
        var decls = new List<SymAlias>();
        decls.Add(TypeDecl());
        while (_curLex.Is(LexType.Identifier))
        {
            decls.Add(TypeDecl());
        }

        return new TypeDeclsNode(decls);
    }
    public VarDeclsNode VarDecls()
    {
        Eat();
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
        var names = new List<IdNode>();
        var ids = new List<IdNode>();
        var symVarParam = new List<SymVarParam>();
        names.Add(Id());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            names.Add(Id());
        }
        Require(LexSeparator.Colon);
        var type = Type();
        foreach (var i in names)
        {
            symVarParam.Add(new SymVarParam(i, type));
        }
        
        ExpressionNode? exp = null;
        if (_curLex.Is(LexOperator.Equal))
        {
            if (ids.Count > 1)
            {
                throw new SyntaxException(_curLex.Pos, "Error initialization");
            }
            Eat();
            exp = Expression();
        }

        Require(LexSeparator.Semicolom);
        return new VarDeclNode(names, symVarParam, exp);
    }
}