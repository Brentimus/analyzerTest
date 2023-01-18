using System.Collections.Specialized;
using System.Data;

namespace Parser.Sym;

public abstract class Sym : ITreePrintable
{
    public Sym(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void PrintTree(string branchAscii)
    {
        throw new NotImplementedException();
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
}
public class SymInteger : SymType
{
    public SymInteger() : base("integer")
    {
    }
}
public class SymDouble : SymType
{
    public SymDouble() : base("double")
    {
    }
}
public class SymBoolean : SymType
{
    public SymBoolean() : base("boolean")
    {
    }
}
public class SymChar : SymType
{
    public SymChar() : base("char")
    {
    }
}
public class SymString : SymType
{
    public SymString() : base("string")
    {
    }
}
public class SymAlias : SymType
{
    public SymType Original { get; }

    public SymAlias(string name, SymType original) : base(name)
    {
        Original = original;
    }

    public override SymType ResolveAlias()
    {
        return Original;
    }
}
public class SymVar : Sym
{
    public SymType SymType { get; }

    public SymVar(string name, SymType symType) : base(name)
    {
        SymType = symType;
    }
}
public class SymConst : Sym
{
    public SymConst(string name) : base(name)
    {
    }
}
public class SymParam : SymVar
{
    public SymParam(string name, SymType symType) : base(name, symType)
    {
    }
}
public class SymVarParam : SymParam
{
    public SymVarParam(string name, SymType symType) : base(name, symType)
    {
    }
}
public class SymConstParam : SymParam
{
    public SymConstParam(string name, SymType symType) : base(name, symType)
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

        // TODO:

        return true;
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
}
public class SymProcedure : Sym
{
    public SymTable Locals { get; }
    public Parser.CompoundStatementNode CompoundStatementNode { get; }

    public SymProcedure(string name, SymTable locals, Parser.CompoundStatementNode compoundStatementNode) : base(name)
    {
        Locals = locals;
        CompoundStatementNode = compoundStatementNode;
    }
}
public class SymFunction : Sym
{
    public SymTable ReturnType { get; }

    public SymFunction(string name, SymTable locals, Parser.CompoundStatementNode compoundStatementNode,
        SymTable returnType) : base(name)
    {
        ReturnType = returnType;
    }
}
public class SymTable
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