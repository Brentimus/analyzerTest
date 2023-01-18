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
        public List<DeclarationNode> Declarations { get; }
        public CompoundStatementNode Compound { get; }

        public BlockNode(List<DeclarationNode> declarations, CompoundStatementNode compound)
        {
            Declarations = declarations;
            Compound = compound;
        }
    }

    public class FunctionHeader : DeclarationNode
    {
        
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

    }
    public class VarDeclsNode : DeclarationNode
    {
        public List<VarDeclNode> Dels { get; }

        public VarDeclsNode(List<VarDeclNode> dels)
        {
            Dels = dels;
        }
    }
    public class ConstDeclsNode : DeclarationNode
    {
        public ConstDeclsNode(List<ConstDeclNode> decls)
        {
            Decls = decls;
        }

        public List<ConstDeclNode> Decls { get; }
    }
    public class ConstDeclNode : DeclarationNode
    {
        public ConstDeclNode(SymConstParam symConstParam, ExpressionNode exp)
        {
            SymConstParam = symConstParam;
            Exp = exp;
        }
        public SymConstParam SymConstParam { get; }
        public ExpressionNode Exp { get; }
    }
    public class TypeDeclsNode : DeclarationNode
    {
        public TypeDeclsNode(List<SymParam> typeDecs)
        {
            TypeDecs = typeDecs;
        }

        public List<SymParam> TypeDecs { get; }
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
            }
            else if (_curLex.Is(LexKeywords.FUNCTION))
            {
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
        if (_curLex.Is(LexSeparator.Colon))
        {
            Eat();
            type = Type();
        }
        Require(LexOperator.Equal);
        var exp = Expression();
        Require(LexSeparator.Semicolom);
        return new ConstDeclNode(new SymConstParam(id.ToString(), type), exp);// TODO: SymConst or SymConstParam ?
    }
    public SymParam TypeDecl()
    {
        var id = Id();
        Require(LexOperator.Equal);
        var type = Type();
        Require(LexSeparator.Semicolom);
        return new SymParam(id.ToString(), type);
    }
    public TypeDeclsNode TypeDecls()
    {
        var decls = new List<SymParam>();
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
            symVarParam.Add(new SymVarParam(i.ToString(), type));
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