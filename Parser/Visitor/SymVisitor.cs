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

    public void Accept(IEnumerable<Parser.IAcceptable> nodes)
    {
        foreach (var node in nodes)
        {
            node.Accept(this);
        }
    }
    private bool IsTypeEqual(SymType left, SymType right, params SymType[] types)
    {
        return types.Any(type => type.Is(left) && type.Is(right));
    }
    
    public void TypeExist(Parser.ExpressionNode exp)
    {
        if (exp.SymType is null) throw new SemanticException(exp.LexCur!.Pos, "it's not return value");
    }

    public void Visit(Parser.ProgramNode node)
    {
        _symStack.AllocBuiltins();
        _symStack.Alloc();
        if (node.Name is not null)
        {
            var programName = new SymProgramName(node.Name.ToString());
            _symStack.Push(node.Name, programName);
        }
        else
        {
            var ProgramName = new SymProgramName("without name");
            _symStack.Push(node.Name, ProgramName);
        }
        node.Block.Accept(this);
    }

    public void Visit(Parser.BinOpExpressionNode node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        var left = node.Left.SymType;
        var right = node.Right.SymType;
        bool overloadException = false;
        switch (node.Op.Value)
        {
            case LexOperator.Add:
                if (!IsTypeEqual(left, right, new SymInteger(), new SymDouble()) &&
                    !IsTypeEqual(left, right, new SymChar(), new SymString()))
                    overloadException = true;
                if (IsTypeEqual(left, right, new SymDouble()))
                    node.SymType = new SymDouble();
                else if (IsTypeEqual(left, right, new SymInteger()))
                    node.SymType = new SymInteger();
                else
                    node.SymType = new SymString();
                break;
            case LexOperator.Sub:
            case LexOperator.Mul:
                if (!IsTypeEqual(left, right, new SymDouble(), new SymInteger()))
                    overloadException = true;
                if (IsTypeEqual(left, right, new SymDouble()))
                    node.SymType = new SymDouble();
                else
                    node.SymType = new SymInteger();
                break;
            case LexOperator.Div:
                if (!IsTypeEqual(left, right, new SymInteger(), new SymDouble()))
                    overloadException = true;
                node.SymType = new SymDouble();
                break;
            case LexKeywords.AND:
            case LexKeywords.OR:
            case LexKeywords.XOR:
                if (!IsTypeEqual(left, right, new SymBoolean()) &&
                    !IsTypeEqual(left, right, new SymInteger()))
                    overloadException = true;
                node.SymType = new SymBoolean();
                break;
            case LexKeywords.DIV:
            case LexKeywords.MOD:
            case LexKeywords.SHR:
            case LexKeywords.SHL:
                if (!IsTypeEqual(left, right, new SymInteger()))
                    overloadException = true;
                node.SymType = new SymInteger();
                break;
        }

        if (overloadException)
        {
            throw new SemanticException(node.LexCur.Pos,
                $"operator is not overloaded: '{left.Name}' {node.Op.Source} '{right.Name}'");
        }
    }

    public void Visit(Parser.UnOpExpressionNode node)
    {
        if (node.SymType is not null) return;
        node.Operand.Accept(this);
        TypeExist(node.Operand);
        var Operand = node.Operand.SymType;
        switch (node.Op.Value)
        {
            case LexOperator.Add:
            case LexOperator.Sub:
                if (Operand.Is(new SymInteger()))
                {
                    node.SymType = new SymInteger();
                }

                if (Operand.Is(new SymDouble()))
                {
                    node.SymType = new SymDouble();
                }
                return;
            case LexKeywords.NOT:
                if (node.Operand.SymType.Is(new SymInteger()))
                {
                    node.SymType = new SymInteger();
                }

                if (node.Operand.SymType.Is(new SymBoolean()))
                {
                    node.SymType = new SymBoolean();
                }
                return;
            default: throw new SemanticException(node.Op.Pos, "Unary operator is not overloaded");
        }
    }

    public void Visit(Parser.RecordAccess node)
    {
        node.RecordVarRef.Accept(this);
        TypeExist(node.RecordVarRef);
        if (node.RecordVarRef.SymType is null)
        {
            throw new SemanticException(node.RecordVarRef.LexCur.Pos, 
                $"{node.RecordVarRef.LexCur.Value} does not allow to take the field");
        }

        if (node.RecordVarRef.SymType is not SymRecord)
        {
            throw new SemanticException(node.RecordVarRef.LexCur.Pos, $"Identifier not found '{node.RecordVarRef.LexCur.Source}'");
        }

        var recordType = node.RecordVarRef.SymType as SymRecord;
        if (!recordType.Fields.Contains(node.Field.ToString()))
        {
            throw new SemanticException(node.Field.LexCur.Pos, $"unknown field {node.Field.LexCur.Source}");
        }

        node.SymType = ((recordType.Fields.Get(node.Field.ToString()) as SymVar)!).Type;

        //node.SymType = node.Field.SymType;
        node.LValue = node.RecordVarRef.LValue;

    }

    public void Visit(Parser.ArrayAccess node)
    {
        node.ArrayId.Accept(this);
        node.ArrayExp.Accept(this);
        TypeExist(node.ArrayExp);
        TypeExist(node.ArrayId);
        if (!(node.ArrayId.SymType is SymArray))
        {
            throw new SemanticException(node.ArrayId.LexCur.Pos, $"{node.ArrayExp.SymType.Name} does not allow to take the index");
        }

        if (!node.ArrayExp.SymType.Is(new SymInteger()))
        {
            throw new SemanticException(node.ArrayExp.LexCur.Pos, "Expected integer expression in array access");
        }
        node.SymType = ((node.ArrayId.SymType as SymArray)!).Type;
        node.LValue = node.ArrayId.LValue;
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
        node.LValue = false;
    }

    public void Visit(Parser.StringNode node)
    {
        node.SymType = new SymString();
        node.LValue = false;
    }

    public void Visit(Parser.BooleanNode node)
    {
        node.SymType = new SymBoolean();
        node.LValue = false;
    }

    public void Visit(Parser.IdNode node)
    {
        var type = _symStack.Get(node.LexCur, node.ToString()) as SymVar;
        node.SymType = type.Type.ResolveAlias();
        node.LValue = true;
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
        node.Compound.Accept(this);
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
        var localTable = new SymTable();
        _symStack.Push(localTable);
        node.Locals.Accept(this);
        node.Block.Accept(this);
        _symStack.Pop();
        _symStack.Push(node.Id, node);
        node.Id.SymType = node.ReturnType;
    }

    public void Visit(SymProcedure node)
    {
        var localTable = new SymTable();
        _symStack.Push(localTable);
        node.Locals.Accept(this);
        node.Block.Accept(this);
        _symStack.Pop();
        _symStack.Push(node.Id, node);
    }

    public void Visit(SymAlias node)
    {
        node.Original.Accept(this);
        node.Id.SymType = node.Original;
        _symStack.Push(node.Id, node);
    }

    public void Visit(Parser.KeywordNode node)
    {
    }

    public void Visit(Parser.CompoundStatementNode node)
    {
        Accept(node.States);
    }

    public void Visit(Parser.FunctionCallStatementNode node)
    {
        Accept(node.Args);
        if (node.Name.LexCur.Is(LexKeywords.WRITE, LexKeywords.WRITELN, LexKeywords.READ, LexKeywords.READLN))
        {
            foreach (var arg in node.Args)
            {
                if ((arg.SymType is SymRecord or SymArray))
                {
                    throw new SemanticException(arg.LexCur.Pos, "Can't read or write variables of this type");
                }
                if (node.Name.LexCur.Is(LexKeywords.READ,LexKeywords.READLN))
                {
                    if (!arg.LValue)
                    {
                        throw new SemanticException(arg.LexCur.Pos, "variable identifier expected");
                    }

                    if (arg.SymType.Is(new SymBoolean()))
                    {
                        throw new SemanticException(arg.LexCur.Pos, "Can't read or write variables of this type");
                    }
                }
            }
            return;
        }

        var symProcedure = _symStack.Get(node.LexCur, node.Name.ToString()) as SymProcedure;
        
        if (node.Args.Count != symProcedure.Locals.Data.Count)
            throw new SemanticException(node.Name.LexCur.Pos, $"call doesn't match header");
        
        for (int i = 0; i < node.Args.Count; ++i)
        {
            var type = ((SymParam) symProcedure.Locals.Data[i]).Type;
            
            //TODO: CAST
            if (!node.Args[i].SymType.Is(type.ResolveAlias()))
            {
                throw new SemanticException(node.Name.LexCur.Pos, $"call doesn't match header");
            }
        }

        if (symProcedure is SymFunction)
            node.Name.SymType = (symProcedure as SymFunction).ReturnType;
    }

    public void Visit(Parser.AssignmentStatementNode node)
    {
        node.VarRef.Accept(this);
        node.Exp.Accept(this);
        if (!node.VarRef.LValue)
        {
            throw new SemanticException(node.VarRef.LexCur.Pos, "lvalue expected");
        }
        TypeExist(node.VarRef);
        TypeExist(node.Exp);
        var left = node.VarRef.SymType;
        if (left is null)
        {
            if (left is SymProcedure)
            {
                throw new SemanticException(node.VarRef.LexCur.Pos, $"{node.VarRef.LexCur.Source} is not expression");
                
            }
            //TODO: FUNC CHECK ?
        }
        
        var right = node.Exp.SymType;
        switch (node.Op.Value)
        {
            case LexOperator.Assign:
                if (IsTypeEqual(left, right,
                        new SymInteger(), new SymDouble(),
                        new SymDouble(), new SymChar(), new SymString()))
                return;
                break;
            case LexOperator.AssignSub:
            case LexOperator.AssignAdd:
            case LexOperator.AssignDiv:
            case LexOperator.AssignMul:
                if (IsTypeEqual(left, right, new SymInteger(), new SymDouble()))
                    return;
                break;
        }
        throw new SemanticException(node.Op.Pos,
            $"operator is not overloaded: '{left.Name}' {node.Op.Source} '{right.Name}'");
    }

    public void Visit(Parser.IfStatementNode node)
    {
        node.Exp.Accept(this);
        if (!node.Exp.SymType.Is(new SymBoolean()))
        {
            throw new SemanticException(node.Exp.LexCur.Pos, "Boolean expected");
        }
    }

    public void Visit(Parser.WhileStatementNode node)
    {
        node.Exp.Accept(this);
        if (!node.Exp.SymType.Is(new SymBoolean()))
        {
            throw new SemanticException(node.Exp.LexCur.Pos, "Boolean expected");
        }
    }

    public void Visit(Parser.ForStatementNode node)
    {
        node.Id.Accept(this);
        if (!node.Id.LValue)
            throw new SemanticException(node.Id.LexCur.Pos, "expect lvalue");
        if (!node.Id.SymType.Is(new SymInteger()))
            throw new SemanticException(node.Id.LexCur.Pos, $"expect type integer but found type {node.Id.SymType.Name}");

        node.ExpFor.Accept(this);
        node.ExpTo.Accept(this);
        Lex check = null;
        if (!node.ExpFor.SymType.Is(new SymInteger()))
        {
            check = node.ExpFor.LexCur;
        }
        else if (!node.ExpTo.SymType.Is(new SymInteger()))
        {
            check = node.ExpTo.LexCur;
        }

        if (check is not null)
        {
            throw new SemanticException(check.Pos, $"expect integer found '{check.Source}'");
        }

        node.State.Accept(this);
    }

    public void Visit(SymType node)
    {
    }

    public void Visit(SymArray node)
    {
        node.Range.Accept(this);
    }

    public void Visit(SymRecord node)
    {
        if (node.Fields.Duplicate is not null)
        {
            throw new SemanticException(node.Fields.Duplicate.LexCur.Pos,
                $"Duplicate '{node.Fields.Duplicate.LexCur.Source}'");
        }
        node.Fields.Accept(this);
    }

    public void Visit(Parser.TypeRangeNode node)
    {
        node.Begin.Accept(this);
        node.End.Accept(this);
        if (!node.Begin.SymType.Is(new SymInteger()))
        {
            throw new SemanticException(node.Begin.LexCur.Pos, "Ordinal expected in array type declaration");
        }
        if (!node.End.SymType.Is(new SymInteger()))
        {
            throw new SemanticException(node.End.LexCur.Pos, "Ordinal expected in array type declaration");
        }
    }

    public void Visit(Parser.FieldSelectionNode node)
    {
        node.Type.Accept(this);
        foreach (var id in node.Ids)
        {
            id.SymType = node.Type;
        }
    }

    public void Visit(Parser.ConstDeclNode node)
    {
        var sym = node.SymConst;
        var id = node.Name;
        node.Exp.Accept(this);
        TypeExist(node.Exp);
        if (sym.Type is null && node.Exp is not null)
            sym.Type = node.Exp.SymType;

        if (sym.Type is not SymArray or not SymRecord)
        {
            if (sym.Type is not null)
                _symStack.Get(id.LexCur,sym.Type.Name);
        }
        _symStack.Push(id, sym);
        
        // TODO: CAST INTEGER TO DOUBLE AND STRING TO CHAR
        if (!node.Exp.SymType.Is(node.SymConst.Type))
        {
            //TODO: FIX LEXCUR.POS
            throw new SemanticException(node.Exp.LexCur.Pos,
                $"incompatible types: got '{node.Exp.SymType.Name}' expected '{node.SymConst.Type.Name}'");
        }
    }

    public void Visit(Parser.VarDeclNode node)
    {
        Parser.IdNode idNode = null!;
        foreach (var item in Enumerable.Zip(node.SymVars, node.Names))
        {
            var sym = item.First;
            var id = item.Second;
            idNode = id;
            if (!(sym.Type is SymArray or SymRecord))
            {
                _symStack.Get(id.LexCur, sym.Type.Name);
            }
            else
                sym.Type.Accept(this);

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
        _symStack.Push(node.Id, node);
        node.Id.Accept(this);
    }

    public void Visit(SymVarParam node)
    {
        _symStack.Push(node.Id, node);
        node.Id.Accept(this);
    }

    public void Visit(SymParam node)
    {
        _symStack.Push(node.Id, node);
        node.Id.Accept(this);
    }

    public void Visit(SymVar node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymTable node)
    {
        foreach (var key in node.Data.Keys)
        {
            (node.Data[key] as SymVar)!.Accept(this);
        }
    }

    public void Visit(Parser.ParamSelectionNode node)
    {
        var modifier = node.Modifier.LexCur;
        var type = node.Type;
        SymVar symParam = null;
        foreach (var id in node.Ids)
        {
            if (modifier is not null)
            {
                if (modifier.Is(LexKeywords.CONST))
                    symParam = new SymConstParam(id, node.Type);
                else
                    symParam = new SymVarParam(id, node.Type);
            }
            else
                symParam = new SymParam(id, node.Type);

            node.Type.Accept(this);
            _symStack.Push(id, symParam);
        }
    }

    public void Visit(Parser.RelOpExpressionNode node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        node.SymType = new SymBoolean();
        TypeExist(node.Left);
        TypeExist(node.Right);
        var left = node.Left.SymType;
        var right = node.Right.SymType;
        if (IsTypeEqual(left,right, new SymDouble(), new SymInteger(), new SymBoolean(), new SymString()) &&
            node.Op.Is(LexOperator.Equal, LexOperator.Less, LexOperator.More, LexOperator.NoEqual, LexOperator.LessEqual,
                LexOperator.MoreEqual))
            return;
        if (left.Is(new SymInteger()) && right.Is(new SymDouble()) ||
           ((right.Is(new SymInteger()) && left.Is(new SymDouble()))))
            return;
        throw new SemanticException(node.Op.Pos, $"Operator is not overloaded: {left.Name} {node.Op.Source} {right.Name}");
    }

    public void Visit(Parser.CharNode node)
    {
        node.SymType = new SymChar();
        node.LValue = false;
    }
}