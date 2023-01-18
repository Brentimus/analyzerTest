using Lexer;
using Parser.Sym;

namespace Parser;

public partial class Parser
{
    public SymType Type()
    {
        var lex = _curLex;
        if (lex.Is(LexType.Identifier))
        {
            return PrimitiveType();
        }
        if (lex.Is(LexKeywords.ARRAY))
        {
            Eat();
            return ArrayType();
        }

        if (lex.Is(LexKeywords.RECORD))
        {
            Eat();
            return RecordType();
        }
        throw new Exception("");
    }

    public SymType PrimitiveType()
    {
        if (_curLex.Is(LexKeywords.STRING))
            return new SymType(_curLex.Value.ToString().ToLower()); //TODO: Maybe wrong
        return new SymType(Id().ToString());
    }

    public SymArray ArrayType()
    {
        Require(LexSeparator.Lbrack);
        var range = TypeRanges();
        Require(LexSeparator.Rbrack);
        Require(LexKeywords.OF);
        var type = Type();
        return new SymArray(type, range);
    }

    public List<TypeRangeNode> TypeRanges()
    {
        var ranges = new List<TypeRangeNode>();
        ranges.Add(TypeRange());
        while (true)
        {
            if (!_curLex.Is(LexSeparator.Comma))
            {
                break;
            }

            Eat();
            ranges.Add(TypeRange());
        }

        return ranges;
    }

    public TypeRangeNode TypeRange()
    {
        var begin = Expression();
        Require(LexSeparator.Doubledot);
        var end = Expression();
        return new TypeRangeNode(begin, end);
    }

    public class TypeRangeNode : Node
    {
        public TypeRangeNode(ExpressionNode begin, ExpressionNode end)
        {
            Begin = begin;
            End = end;
        }

        public ExpressionNode Begin { get; }
        public ExpressionNode End { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public SymRecord RecordType()
    {
        var fieldList = new List<FieldSelectionNode>();
        Require(LexKeywords.RECORD);
        if (!_curLex.Is(LexKeywords.END))
        {
            fieldList = FieldList();
        }

        Require(LexKeywords.END);
        var table = new SymTable();
        foreach (var field in fieldList)
        {
            foreach (var idNode in field.Ids)
            {
                // TODO: get name in lower
                table.Push(new SymVar(idNode, field.Type), true);
            }
        }

        return new SymRecord(table);
    }

    public List<FieldSelectionNode> FieldList()
    {
        var fields = new List<FieldSelectionNode>();
        fields.Add(FieldSelection());
        while (true)
        {
            if (!_curLex.Is(LexSeparator.Semicolom))
            {
                break;
            }

            Eat();
            fields.Add(FieldSelection());
        }

        return fields;
    }
    public FieldSelectionNode FieldSelection()
    {
        var ids = IdList();
        Require(LexSeparator.Colon);
        var type = Type();
        return new FieldSelectionNode(ids, type);
    }

    public class FieldSelectionNode : Node
    {
        public FieldSelectionNode(List<IdNode> ids, SymType type)
        {
            Ids = ids;
            Type = type;
        }

        public List<IdNode> Ids { get; }
        public SymType Type { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}