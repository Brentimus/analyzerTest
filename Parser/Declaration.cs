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
        public BlockNode(List<DeclarationNode> declarations, CompoundStatementNode compound, Lex lex = null) : base(lex)
        {
            Declarations = declarations;
            Compound = compound;
        }

        public List<DeclarationNode> Declarations { get; }
        public CompoundStatementNode Compound { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class ProcDelcNode : DeclarationNode
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
        private SymFunction Function { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class VarDeclNode : DeclarationNode
    {
        public VarDeclNode(List<SymVarParam> symVarParams, ExpressionNode? exp)
        {
            SymVarParams = symVarParams;
            Exp = exp;
        }

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
        public ConstDeclNode(SymConst symConst, SymConstParam symConstParam, ExpressionNode exp)
        {
            SymConst = symConst;
            SymConstParam = symConstParam;
            Exp = exp;
        }

        public SymConst SymConst { get; }
        public SymConstParam SymConstParam { get; }
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
    public class ParameterNode : DeclarationNode
    {
        public ParameterNode(KeywordNode keyword, List<IdNode> idList, SymType type)
        {
            Keyword = keyword;
            IdList = idList;
            Type = type;
        }
        public KeywordNode Keyword { get; }
        public List<IdNode> IdList { get; }
        public SymType Type { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public ParameterNode Parameter()
    {
        KeywordNode keyword = null;
        if (_curLex.Is(LexKeywords.VAR, LexKeywords.CONST))
        {
            keyword = Keyword();
        }

        var idList = IdList();
        Require(LexSeparator.Colon);
        var type = Type();
        return new ParameterNode(keyword, idList, type);
    }
    public SymFunction FuncDecl()
    {
        IdNode id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParameterNode>();
        locals.Add(Parameter());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            locals.Add(Parameter());
        }
        
        Require(LexSeparator.Rparen);
        Require(LexSeparator.Colon);
        var type = Type();
        Require(LexSeparator.Semicolom);
        while (true)
        {
            if (_curLex.Is(LexKeywords.CONST))
            {
                ConstDecls();
            } else if (_curLex.Is(LexKeywords.VAR))
            {
                VarDecls();
            }else if (_curLex.Is(LexKeywords.TYPE))
            {
                TypeDecls();
            } else break;
        }

        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        {
            foreach (var idNode in local.IdList)
            {
                table.Push(new SymVar(idNode, local.Type), true);
            }
        }
        return new SymFunction(id, table, compound, type);
    }
    public SymProcedure ProcDecl()
    {
        IdNode id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParameterNode>();
        locals.Add(Parameter());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            locals.Add(Parameter());
        }
        Require(LexSeparator.Rparen);
        Require(LexSeparator.Semicolom);
        while (true)
        {
            if (_curLex.Is(LexKeywords.CONST))
            {
                ConstDecls();
            } else if (_curLex.Is(LexKeywords.VAR))
            {
                VarDecls();
            }else if (_curLex.Is(LexKeywords.TYPE))
            {
                TypeDecls();
            } else break;
        }
        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        {
            foreach (var idNode in local.IdList)
            {
                table.Push(new SymVar(idNode, local.Type), true);
            }
        }
        return new SymProcedure(id, table, compound);
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
                Eat();
                declarations.Add(TypeDecls());
            }
            else if (_curLex.Is(LexKeywords.CONST))
            {
                Eat();
                declarations.Add(ConstDecls());
            }
            else if (_curLex.Is(LexKeywords.PROCEDURE))
            {
                Eat();
                declarations.Add(new ProcDelcNode(ProcDecl()));
            }
            else if (_curLex.Is(LexKeywords.FUNCTION))
            {
                Eat();
                declarations.Add(new FuncDelcNode(FuncDecl()));
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
        SymConst symConst = null;
        SymConstParam symConstParam = null;
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
            return new ConstDeclNode(new SymConst(id), null, null);
        }
        return new ConstDeclNode(null, new SymConstParam(id, type), exp);// TODO: SymConst or SymConstParam ?
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
        var symVarParam = new List<SymVarParam>();
        ids.Add(Id());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            ids.Add(Id());
        }
        Require(LexSeparator.Colon);
        var type = Type();
        
        foreach (var i in ids)
        {
            symVarParam.Add(new SymVarParam(i, type));
        }
        
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
        return new VarDeclNode(symVarParam, exp);
    }
}