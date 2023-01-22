using System.Collections;
using System.Collections.Specialized;
using Parser.Visitor;
using Lexer;

namespace Parser.Sym;

public abstract class Sym : Parser.IAcceptable
{
    public Sym(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public abstract void Accept(IVisitor visitor);

    public override string ToString()
    {
        return Name;
    }
}

public class SymType : Sym
{
    public SymType(string name) : base(name)
    {
    }

    public virtual SymType ResolveAlias()
    {
        return this;
    }

    public virtual bool Is(SymType other)
    {
        return ResolveAlias().Name.Equals(other.ResolveAlias().Name);
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "type";
    }
}

public class SymInteger : SymType
{
    public SymInteger() : base("integer")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymDouble : SymType
{
    public SymDouble() : base("double")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymBoolean : SymType
{
    public SymBoolean() : base("boolean")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymChar : SymType
{
    public SymChar() : base("char")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymString : SymType
{
    public SymString() : base("string")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymAlias : SymType
{
    public SymType Original { get; }
    public Parser.IdNode Id { get; }

    public SymAlias(Parser.IdNode id, SymType original) : base(id.ToString())
    {
        Id = id;
        Original = original;
    }

    public override SymType ResolveAlias()
    {
        return Original;
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "alias";
    }
}

public class SymVar : Sym
{
    public SymType Type { get; set; }

    public SymVar(Parser.IdNode id, SymType type) : base(id.ToString())
    {
        Type = type;
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "var";
    }
}

public class SymConst : SymVar
{
    public SymConst(Parser.IdNode id, SymType type) : base(id, type)
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "const";
    }
}

public class SymVarParam : SymParam
{
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public SymVarParam(Parser.IdNode id, SymType type) : base(id, type)
    {
    }

    public override string ToString()
    {
        return "var param";
    }
}

public class SymConstParam : SymParam
{
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public SymConstParam(Parser.IdNode id, SymType type) : base(id, type)
    {
    }

    public override string ToString()
    {
        return "const param";
    }
}

public class SymRecord : SymType
{
    public SymTable Fields { get; }

    public SymRecord(SymTable fields) : base("record")
    {
        Fields = fields;
    }

    public override bool Is(SymType other)
    {
        if (other is not SymRecord)
        {
            return false;
        }

        var otherCasted = other as SymRecord;
        foreach (string field in Fields.Data.Keys)
        {
            var sym = Fields.Get(field) as SymVar;
            if (!otherCasted!.Fields.Contains(sym!.Name))
            {
                return false;
            }

            var otherSym = otherCasted.Fields.Get(sym.Name) as SymVar;

            if (!sym.Type.Is(otherSym!.Type))
            {
                return false;
            }
        }

        return true;
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "record";
    }
}

public class SymArray : SymType
{
    public SymType Type { get; }
    public Parser.TypeRangeNode Range { get; }

    public SymArray(SymType type, Parser.TypeRangeNode range) : base("array")
    {
        Type = type;
        Range = range;
    }

    public override bool Is(SymType other)
    {
        return other is SymArray && Type.Is((other as SymArray)!.Type);
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "array";
    }
}

public class SymParam : SymVar
{
    public SymParam(Parser.IdNode id, SymType type) : base(id, type)
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "param";
    }
}

public class SymProcedure : Sym
{
    public SymProcedure(Parser.IdNode id, SymTable locals, Parser.BlockNode block) : base(id.ToString())
    {
        Locals = locals;
        Block = block;
        IsForward = true;
    }

    public SymTable Locals { get; set; }
    public Parser.BlockNode Block { get; set; }
    public bool IsForward { get; set; }


    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "procedure";
    }
}

public class SymFunction : SymProcedure
{
    public SymFunction(Parser.IdNode id, SymTable locals, Parser.BlockNode block, SymType returnType) : base(id, locals,
        block)
    {
        ReturnType = returnType;
    }

    public SymType ReturnType { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "function";
    }
}

public class SymTable : Parser.IAcceptable
{
    public OrderedDictionary Data { get; }

    public SymTable()
    {
        Data = new OrderedDictionary();
    }

    public void Push(Parser.IdNode id, Sym sym, bool is_parser = false)
    {
        if (is_parser && Contains(sym.Name))
        {
            return;
        }

        if (Contains(sym.Name))
        {
            throw new SemanticException(id.LexCur.Pos, $"Duplicate identifier {sym.Name}");
        }

        Data.Add(sym.Name, sym);
    }

    public void Border(String[] texts, int wight)
    {
        string textInString = "";
        foreach (var text in texts)
        {
            int space = text.Length > (wight / 4) ? 0 : (wight / 4) - text.Length;

            textInString += " │ " + text + " ".PadRight(space);
        }

        Console.Out.Write(textInString);
        Console.WriteLine(new string(' ', wight - 1 - textInString.Length) + "│");
    }

    public void Print(SymTable table)
    {
        Console.WriteLine();
        int wight = 100;
        Console.WriteLine(" " + new string('─', wight - 1));
        foreach (var key in table.Data.Keys)
        {
            string value = "";
            var test = (table.Data[key] as SymVar);
            value = table.Data[key] as SymVar is null
                ? (table.Data[key] as SymType).ResolveAlias().Name
                : (table.Data[key] as SymVar).Type.ResolveAlias().Name;

            string[] data = {key.ToString(), table.Data[key].ToString(), value};
            Border(data, wight);
        }

        Console.WriteLine(" " + new string('─', wight - 1));
    }

    public void Push(String name, Sym sym)
    {
        if (Contains(name))
        {
            throw new Exception();
        }

        Data.Add(name, sym);
    }

    public Sym? Get(string name)
    {
        if (Data.Contains(name))
        {
            return (Sym) Data[name];
        }

        return null;
    }

    public void Del(string name)
    {
        Data.Remove(name);
    }

    public bool Contains(String name)
    {
        return Data.Contains(name);
    }

    public void Alloc()
    {
        Data.Add("integer", new SymInteger());
        Data.Add("char", new SymChar());
        Data.Add("string", new SymString());
        Data.Add("double", new SymDouble());
        Data.Add("boolean", new SymBoolean());
    }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
/*
 * var b: integer; // stack.Push(sym);
 * function a(b: integer; d: double): integer; ...
 * begin end.
 */

/*
 * stack:
 *      integer
 *      double
 *      a
 *          b
 *          d
 */

public class SymStack
{
    public List<SymTable> Data { get; }

    public SymStack()
    {
        Data = new List<SymTable>();
    }

    public void Push(SymTable data)
    {
        Data.Add(data);
    }

    public void Alloc()
    {
        Data.Add(new SymTable());
    }

    public void AllocBuiltins()
    {
        var a = new SymTable();
        a.Alloc();
        Data.Add(a);
    }

    public void Push(Parser.IdNode id, Sym sym)
    {
        Data[^1].Push(id, sym);
    }

    public Sym? Get(string name)
    {
        for (var i = Data.Count - 1; i >= 0; i--)
            if (Data[i].Contains(name))
                return Data[i].Get(name);

        throw new Exception();
    }

    public void Pop()
    {
        Data.Remove(Data[^1]);
    }

    public void Print(SymStack stack)
    {
        foreach (var data in stack.Data)
        {
            data.Print(data);
        }
    }
}