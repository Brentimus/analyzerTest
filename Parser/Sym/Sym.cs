using System.Collections;
using System.Collections.Specialized;
using Parser.Visitor;

namespace Parser.Sym;

public abstract class Sym : Parser.IAcceptable
{
    public Sym(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public abstract void Accept(IVisitor visitor);
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

    public SymAlias(Parser.IdNode id, SymType original) : base(id.ToString())
    {
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
}

public class SymVar : Sym
{
    public SymType Type { get; }
    public SymVar(Parser.IdNode id, SymType type) : base(id.ToString())
    {
        Type = type;
    }
    
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymConst : SymVar
{
    public SymConst(Parser.IdNode id, SymType type) : base(id, type)
    {
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
        if (!(other is SymRecord))
        {
            return false;
        }

        // TODO: ?

        return true;
    }
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
public class SymArray : SymType
{
    public SymType Type { get; }
    public List<Parser.TypeRangeNode> Range { get; }
    public SymArray(SymType type, List<Parser.TypeRangeNode> range) : base("array")
    {
        Type = type;
        Range = range;
    }
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
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
}
public class SymProcedure : Sym
{
    public SymProcedure(Parser.IdNode id,SymTable locals, Parser.BlockNode block) : base(id.ToString())
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
}
public class SymFunction : SymProcedure
{
    public SymFunction(Parser.IdNode id, SymTable locals, Parser.BlockNode block, SymType returnType) : base(id, locals, block)
    {
        ReturnType = returnType;
    }
    public SymType ReturnType { get; }
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
public class SymTable : Parser.IAcceptable
{
    public OrderedDictionary Data { get; }

    public SymTable()
    {
        Data = new OrderedDictionary();
    }

    public void Push(Sym sym, bool is_parser = false)
    {
        if (is_parser && Contains(sym.Name))
        {
            return;
        }

        if (Contains(sym.Name))
        {
            throw new Exception();
        }

        Data.Add(sym.Name, sym);
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

    void Push(SymTable data)
    {
        Data.Add(data);
    }

    void Alloc()
    {
        Data.Add(new SymTable());
    }

    void AllocBuiltins()
    {
        var a = new SymTable();
        a.Alloc();
        Data.Add(a);
    }

    void Push(Sym sym)
    {
        Data[^1].Push(sym);
    }

    Sym? Get(string name)
    {
        for (var i = Data.Count - 1; i >= 0; i--)
            if (Data[i].Contains(name))
                return Data[i].Get(name);

        throw new Exception();
    }

    void Pop()
    {
        Data.Remove(Data[^1]);
    }
}