using System.Collections;
using System.Collections.Specialized;
using Lexer;
using Parser.Sym;

namespace Parser;

public class PrinterVisitor : IVisitor
{
    private int depth = 0;
    public void PrintDepth()
    {
        Console.Write("".PadRight(depth * 3, ' '));
    }

    public void Print(string str)
    {
        PrintDepth();
        Console.WriteLine(str);
    }
    
    public void Print(IEnumerable<Parser.IAcceptable> nodes)
    {
        foreach (var node in nodes)
        {
            node.Accept(this);
        }
    }
    public void Print(SymTable nodeFields)
    {
        foreach (var i in nodeFields.Data.Values)
        {
            PrintDepth();
            Console.WriteLine(i);
        }
        foreach (var i in nodeFields.Data.Keys)
        {
            PrintDepth();
            Console.WriteLine(i);
        }
    }
    public void Print(Lex lex)
    {
        PrintDepth();
        Console.WriteLine(lex.Value.ToString().ToLower());
    } 
    
    public void Visit(Parser.ProgramNode node)
    {
        Print("main");
        if (node.Name is not null)
        {
            node.Name.Accept(this);
        }
        else
        {
            depth++;
            Print("no name");
            depth--;
        }
        node.Block.Accept(this);
    }

    public void Visit(Parser.BinOpExpressionNode node)
    {
        depth++;
        Print(node.Op);
        node.Left.Accept(this);
        node.Right.Accept(this);
        depth--;
    }
    
    public void Visit(Parser.RelOpExpressionNode node)
    {
        depth++;
        Print(node.Op);
        node.Left.Accept(this);
        node.Right.Accept(this);
        depth--;
    }

    public void Visit(Parser.UnOpExpressionNode node)
    {
        depth++;
        Print(node.Op);
        node.Node.Accept(this);
        depth--;
    }

    public void Visit(Parser.RecordAccess node)
    {
        depth++;
        Print("record access");
        node.RecordId.Accept(this);
        node.Field.Accept(this);
        depth--;
    }

    public void Visit(Parser.ArrayAccess node)
    {
        depth++;
        Print("array access");
        node.ArrayId.Accept(this);
        node.ArrayExp.Accept(this);
        depth--;
    }

    public void Visit(Parser.CallNode node)
    {
        depth++;
        Print("call");
        node.Name.Accept(this);
        Print(node.Args);
        depth--;
    }

    public void Visit(Parser.WriteCallNode node)
    {
        depth++;
        if(node.NewLine)
            Print("writeln");
        else
            Print("write");
        Print(node.Args);
        depth--;
    }

    public void Visit(Parser.ReadCallNode node)
    {
        depth++;
        if(node.NewLine)
            Print("readln");
        else
            Print("read");
        Print(node.Args);
        depth--;
    }

    public void Visit(Parser.NumberExpressionNode node)
    {
        depth++;
        Print(node.LexCur);
        depth--;
    }

    public void Visit(Parser.StringNode node)
    {
        depth++;
        Print(node.LexCur);
        depth--;
    }

    public void Visit(Parser.BooleanNode node)
    {
        depth++;
        Print(node.LexCur);
        depth--;
    }

    public void Visit(Parser.IdNode node)
    {
        depth++;
        Print(node.LexCur);
        depth--;
    }

    public void Visit(SymInteger node)
    {
        depth++;
        Print(node.Name);
        depth--;
    }

    public void Visit(SymDouble node)
    {
        depth++;
        Print(node.Name);
        depth--;
    }

    public void Visit(SymChar node)
    {
        depth++;
        Print(node.Name);
        depth--;
    }

    public void Visit(SymString node)
    {
        depth++;
        Print(node.Name);
        depth--;
    }

    public void Visit(SymBoolean node)
    {
        depth++;
        Print(node.Name);
        depth--;
    }

    public void Visit(Parser.BlockNode node)
    {
        depth++;
        Print("decls");
        Print(node.Declarations);
        Print("block");
        node.Compound.Accept(this);
        depth--;
    }

    public void Visit(Parser.ConstDeclsNode node)
    {
        depth++;
        Print("consts");
        Print(node.Decls);
        depth--;
    }
    public void Visit(Parser.ConstDeclNode node)
    {
        depth++;
        Print("const");
        node.SymConst.Accept(this);
        node.Exp.Accept(this);
        depth--;
    }

    public void Visit(Parser.VarDeclsNode node)
    {
        depth++;
        Print("vars");
        Print(node.Dels);
        depth--;
    }
    public void Visit(Parser.VarDeclNode node)
    {
        depth++;
        Print("var");
        Print(node.names);
        Print(node.SymVarParams);
        if (node.Exp is not null)
        {
            node.Exp.Accept(this);
        }
        else
        {
            Print("no exp");
        }
        depth--;
    }

    public void Visit(Parser.TypeDeclsNode node)
    {
        depth++;
        Print("def types");
        Print(node.TypeDecs);
        depth--;
    }

    public void Visit(SymFunction node)
    {
        depth++;
        Print("func decl");
            depth++;
            Print(node.Name);
            depth--;
        node.ReturnType.Accept(this);
        Print(node.Locals);
        node.Compound.Accept(this);
        depth--;
    }

    public void Visit(SymProcedure node)
    {
        depth++;
        Print("proc decl");
            depth++;
            Print(node.Name);
            depth--;
        Print(node.Locals);
        node.Compound.Accept(this);
        depth--;
    }
    public void Visit(SymAlias node)
    {
        depth++;
        Print("type def");
            depth++;
            Print(node.Name);
            depth--;
        node.Original.Accept(this);
       depth--;
    }

    public void Visit(Parser.KeywordNode node)
    {
        depth++;
        PrintDepth();
        Print(node.LexCur);
        depth--;
    }

    public void Visit(Parser.CompoundStatementNode node)
    {
        depth++;
        Print("compounds statements");
        Print(node.States);
        depth--;
    }

    public void Visit(Parser.FunctionCallStatementNode node)
    {
        depth++;
        Print("call");
        node.Name.Accept(this);
            depth++;
            Print(node.Args);
            depth--;
        depth--;
    }

    public void Visit(Parser.AssignmentStatementNode node)
    {
        depth++;
        Print(node.Op);
        node.VarRef.Accept(this);
        node.Exp.Accept(this);
        depth--;
    }

    public void Visit(Parser.IfStatementNode node)
    {
        depth++;
        Print("if");
        node.Exp.Accept(this);
        node.StateThen.Accept(this);
        Print("else");
        if (node.StateElse is not null)
        {
            node.StateElse.Accept(this);
        }
        else
        {
            depth++;
            Print("empty");
            depth--;
        }
        depth--;
    }

    public void Visit(Parser.WhileStatementNode node)
    {
        depth++;
        Print("while");
        node.Exp.Accept(this);
        node.State.Accept(this);
        depth--;
    }

    public void Visit(Parser.ForStatementNode node)
    {
        depth++;
        Print("for");
        node.Id.Accept(this);
        node.ExpFor.Accept(this);
        node.To.Accept(this);
        node.ExpTo.Accept(this);
        node.State.Accept(this);
        depth--;
    }

    public void Visit(SymType node)
    {
        depth++;
        Print("type");
            depth++;
            Print(node.Name);
            depth--;
        depth--;
    }

    public void Visit(SymArray node)
    {
        depth++;
        Print("array type");
        node.Type.Accept(this);
        Print(node.Range);
        depth--;
    }

    public void Visit(SymRecord node)
    {
        depth++;
        Print("record");
        Print(node.Fields);
        depth--;
    }
    public void Visit(Parser.TypeRangeNode node)
    {
        depth++;
        Print("range");
        node.Begin.Accept(this);
        node.End.Accept(this);
        depth--;
    }

    public void Visit(Parser.FieldSelectionNode node)
    {
        depth++;
        Print("fields");
        node.Type.Accept(this);
        Print(node.Ids);
        depth--;
    }
    public void Visit(Parser.StatementNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymConstParam node)
    {
        depth++;
        if (node.Type is null)
        {
            node.Type.Accept(this);
        }
        else
        {
            Print("no type");
        }
        depth--;
    }

    public void Visit(SymVarParam node)
    {
        depth++;
        if (node.Type is null)
        {
            Print("no type");
        }
        else
        {
            node.Type.Accept(this);
        }
        depth--;
    }

    public void Visit(SymParam node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SymConst node)
    {
        
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
        depth++;
        Print("param");
        if (node.Modifier is null)
        {
            node.Modifier.Accept(this);
        }
        else
        {
            Print("no modifier");
        }
        Print(node.Ids);
        node.Type.Accept(this);
        depth--;
    }
}