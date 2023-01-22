using Lexer;
using Parser.Sym;

namespace Parser.Visitor;

public class SymVisitor : IVisitor
{
    private SymStack _symStack;
    private bool _inScope;

    public SymVisitor(SymStack symStack)
    {
        _symStack = symStack;
    }
    public void Visit(Parser.ProgramNode node)
    {
        _symStack.AllocBuiltins();
        _symStack.Alloc();
        node.Name?.Accept(this);
        node.Block.Accept(this);
    }
    
    public void Accept(IEnumerable<Parser.IAcceptable> nodes)
    {
        foreach (var node in nodes)
        {
            node.Accept(this);
        }
    }

    public void Visit(Parser.BinOpExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.UnOpExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.RecordAccess node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.ArrayAccess node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.CallNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.NumberExpressionNode node)
    {
        if (node.LexCur.Is(LexType.Integer))
        {
            node.SymType = new SymInteger();
        }
        else
        {
            node.SymType = new SymDouble();
        }
    }

    public void Visit(Parser.StringNode node)
    {
        node.SymType = new SymString();
    }

    public void Visit(Parser.BooleanNode node)
    {
        node.SymType = new SymBoolean();
    }

    public void Visit(Parser.IdNode node)
    {
        //TODO: func check?
    }

    public void Visit(SymConst node)
    {
        throw new NotImplementedException();
    }
    public void Visit(SymInteger node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymDouble node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymChar node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymString node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymBoolean node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.BlockNode node)
    {
        foreach (var decls in node.Declarations) // TODO: forward procedure
        {
            decls.Accept(this);
        }
        // procedure a(a: integer;); begin end;
        // function a(a: integer;): integer; begin a:= 10; end;
        // for a[0] := 0 to 10 do;
        //node.Compound.Accept(this);
    }

    public void Visit(Parser.ConstDeclsNode node)
    {
        Accept(node.Decls);
    }

    public void Visit(Parser.VarDeclsNode node)
    {
        Accept(node.Decls);
    }

    public void Visit(Parser.TypeDeclsNode node)
    {
        Accept(node.TypeDecls);
    }

    public void Visit(SymFunction node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymProcedure node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymAlias node)
    {
        node.Original.Accept(this);
        node.Id.SymType = node.Original;
        _symStack.Push(node.Id, node);
    }

    public void Visit(Parser.KeywordNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.CompoundStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.FunctionCallStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.AssignmentStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.IfStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.WhileStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.ForStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymType node)
    {
        //TODO : maybe wrong
        //throw new NotImplementedException();
    }

    public void Visit(SymArray node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymRecord node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.TypeRangeNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.FieldSelectionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.ConstDeclNode node)
    {
        var sym = node.SymConst;
        var id = node.Name;
        node.Exp.Accept(this);
        if (sym.Type is null && node.Exp is not null)
            sym.Type = node.Exp.SymType;
        
        if (sym.Type is not SymArray or not SymRecord)
        {
            if (sym.Type is not null)
                _symStack.Get(sym.Type.Name);
        }
        
        _symStack.Push(id, sym);
        
        
        // TODO: CAST INTEGER TO DOUBLE AND STRING TO CHAR
        if (!node.Exp.SymType.Is(node.SymConst.Type))
        {
            //TODO: FIX LEXCUR.POS
            throw new SemanticException(id.LexCur.Pos,
                $"incompatible types: got '{node.Exp.SymType.Name}' expected '{node.SymConst.Type.Name}'");
        }
    }

    public void Visit(Parser.VarDeclNode node)
    {
        Parser.IdNode idNode = null!;
        foreach (var item in Enumerable.Zip(node.SymVars, node.Names))
        {
            var sym = item.First as SymVar;
            var id = item.Second as Parser.IdNode;
            idNode = id;
            if (sym.Type is not SymArray or not SymRecord)
            {
                _symStack.Get(sym.Type.Name);
            }
            _symStack.Push(id, sym);
        }

        if (node.Exp is not null)
        {
            node.Exp.Accept(this);
            // TODO: CAST INTEGER TO DOUBLE
            if (!node.Exp.SymType.Is(node.SymVars[0].Type))
            {
                throw new SemanticException(idNode.LexCur.Pos,
                    $"incompatible types: got '{node.Exp.SymType.Name}' expected '{node.SymVars[0].Type.Name}'");
            }
        }
    }

    public void Visit(SymConstParam node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymVarParam node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymParam node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymVar node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymTable node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.ParamSelectionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.RelOpExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(Parser.CharNode node)
    {
        node.SymType = new SymChar();
    }
}